using Bloomy.Models;
using Bloomy.Models.Enums;
using BloomyBE.DTOs.AI;
using BloomyBE.Helpers;
using BloomyBE.Repositories.Interfaces;
using BloomyBE.Services.Interfaces;
using BloomyBE.Services.Prompts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using BloomyBE.Configuration;
using System.Text.Json;

namespace BloomyBE.Services.Interfaces
{
    public interface IAIService
    {
        Task<AIChatResponseDto> ChatAsync(Guid userId, AIChatRequestDto dto, CancellationToken ct = default);
        IAsyncEnumerable<string> StreamChatAsync(Guid userId, AIChatRequestDto dto, Guid assistantMessageId, CancellationToken ct = default);
        Task<AIAnalyzeImageResponseDto> AnalyzeImageAsync(Guid userId, IFormFile image, Guid? conversationId, CancellationToken ct = default);
        Task<AIGenerateConceptResponseDto> GenerateConceptAsync(Guid userId, AIGenerateConceptRequestDto dto, CancellationToken ct = default);
        Task<List<AIConversationHistoryDto>> GetHistoryAsync(Guid userId, Guid? conversationId, CancellationToken ct = default);
        Task<SaveAIConceptResponseDto> SaveConceptAsync(Guid userId, SaveAIConceptRequestDto dto, CancellationToken ct = default);
        Task<List<SavedConceptListItemDto>> GetSavedConceptsAsync(Guid userId, CancellationToken ct = default);
        Task DeleteSavedConceptAsync(Guid userId, Guid conceptId, CancellationToken ct = default);
        Task<(Guid ConversationId, Guid AssistantMessageId)> PrepareStreamChatAsync(Guid userId, AIChatRequestDto dto, CancellationToken ct = default);
        Task FinalizeStreamChatAsync(Guid userId, Guid conversationId, Guid assistantMessageId, string fullResponse, CancellationToken ct = default);
    }
}

namespace BloomyBE.Services
{
    public class AIService : IAIService
    {
        private readonly IAIRepository _repo;
        private readonly IGeminiService _gemini;
        private readonly IAIQuotaService _quota;
        private readonly IMemoryCache _cache;
        private readonly GeminiSettings _settings;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AIService> _logger;

        public AIService(
            IAIRepository repo,
            IGeminiService gemini,
            IAIQuotaService quota,
            IMemoryCache cache,
            IOptions<GeminiSettings> settings,
            IWebHostEnvironment env,
            ILogger<AIService> logger)
        {
            _repo = repo;
            _gemini = gemini;
            _quota = quota;
            _cache = cache;
            _settings = settings.Value;
            _env = env;
            _logger = logger;
        }

        public async Task<AIChatResponseDto> ChatAsync(Guid userId, AIChatRequestDto dto, CancellationToken ct = default)
        {
            await _quota.EnsureQuotaAsync(userId, AIUsageType.Chat, ct);
            var conversation = await GetOrCreateConversationAsync(userId, dto.ConversationId, ct);

            var userMsg = await _repo.AddMessageAsync(new AIMessage
            {
                ConversationId = conversation.Id,
                Role = AIMessageRole.User,
                MessageType = AIMessageType.Text,
                Content = dto.Message.Trim()
            }, ct);

            var history = BuildChatHistory(conversation);
            history.Add(new GeminiContentPart { Role = "user", Text = dto.Message.Trim() });

            var raw = await _gemini.GenerateContentAsync(AIPrompts.DecorConsultantSystem, history, ct: ct);
            var (visible, metadataDoc) = AIResponseParser.SplitChatResponse(raw);
            var metadata = ParseChatMetadata(metadataDoc);

            await UpdateConversationFromMetadata(conversation, metadata, ct);

            var assistantMsg = await _repo.AddMessageAsync(new AIMessage
            {
                ConversationId = conversation.Id,
                Role = AIMessageRole.Assistant,
                MessageType = AIMessageType.Text,
                Content = visible,
                MetadataJson = metadataDoc?.RootElement.GetRawText()
            }, ct);

            await _quota.RecordUsageAsync(userId, AIUsageType.Chat, ct: ct);

            return new AIChatResponseDto
            {
                ConversationId = conversation.Id,
                UserMessageId = userMsg.Id,
                AssistantMessageId = assistantMsg.Id,
                Reply = visible,
                IsReadyForConcept = metadata?.IsReadyForConcept ?? false,
                GatheredRequirements = metadata?.GatheredRequirements,
                MissingInfo = metadata?.MissingInfo?.ToArray()
            };
        }

        public async Task<(Guid ConversationId, Guid AssistantMessageId)> PrepareStreamChatAsync(
            Guid userId, AIChatRequestDto dto, CancellationToken ct = default)
        {
            await _quota.EnsureQuotaAsync(userId, AIUsageType.Chat, ct);
            var conversation = await GetOrCreateConversationAsync(userId, dto.ConversationId, ct);

            await _repo.AddMessageAsync(new AIMessage
            {
                ConversationId = conversation.Id,
                Role = AIMessageRole.User,
                MessageType = AIMessageType.Text,
                Content = dto.Message.Trim()
            }, ct);

            var assistantMsg = await _repo.AddMessageAsync(new AIMessage
            {
                ConversationId = conversation.Id,
                Role = AIMessageRole.Assistant,
                MessageType = AIMessageType.Text,
                Content = string.Empty
            }, ct);

            return (conversation.Id, assistantMsg.Id);
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            Guid userId, AIChatRequestDto dto, Guid assistantMessageId,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            var conversation = await _repo.GetConversationWithMessagesAsync(
                dto.ConversationId ?? throw new InvalidOperationException("ConversationId required"),
                userId, ct) ?? throw new InvalidOperationException("Cuộc hội thoại không tồn tại.");

            var history = BuildChatHistory(conversation);
            var lastUser = conversation.Messages.LastOrDefault(m => m.Role == AIMessageRole.User);
            if (lastUser != null && !history.Any(h => h.Text == lastUser.Content))
                history.Add(new GeminiContentPart { Role = "user", Text = lastUser.Content });

            await foreach (var chunk in _gemini.StreamContentAsync(AIPrompts.DecorConsultantSystem, history, ct))
                yield return chunk;
        }

        public async Task FinalizeStreamChatAsync(
            Guid userId, Guid conversationId, Guid assistantMessageId, string fullResponse, CancellationToken ct = default)
        {
            var conversation = await _repo.GetConversationWithMessagesAsync(conversationId, userId, ct)
                ?? throw new InvalidOperationException("Cuộc hội thoại không tồn tại.");

            var (visible, metadataDoc) = AIResponseParser.SplitChatResponse(fullResponse);
            var metadata = ParseChatMetadata(metadataDoc);
            await UpdateConversationFromMetadata(conversation, metadata, ct);

            var assistant = conversation.Messages.FirstOrDefault(m => m.Id == assistantMessageId)
                ?? throw new InvalidOperationException("Tin nhắn không tồn tại.");

            assistant.Content = visible;
            assistant.MetadataJson = metadataDoc?.RootElement.GetRawText();
            await _repo.UpdateMessageAsync(assistant, ct);

            await _quota.RecordUsageAsync(userId, AIUsageType.Chat, ct: ct);
        }

        public async Task<AIAnalyzeImageResponseDto> AnalyzeImageAsync(
            Guid userId, IFormFile image, Guid? conversationId, CancellationToken ct = default)
        {
            ValidateImage(image);
            await _quota.EnsureQuotaAsync(userId, AIUsageType.ImageAnalysis, ct);

            var conversation = await GetOrCreateConversationAsync(userId, conversationId, ct);
            var imageUrl = await SaveUploadedImageAsync(image, ct);
            conversation.UploadedSpaceImageUrl = imageUrl;

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, ct);
            var base64 = Convert.ToBase64String(ms.ToArray());
            var mimeType = image.ContentType;

            var analysisJson = await _gemini.AnalyzeImageAsync(
                AIPrompts.SpaceAnalysisSystem, base64, mimeType, ct: ct);

            var analysis = AIResponseParser.ParseJson<SpaceAnalysisDto>(analysisJson) ?? new SpaceAnalysisDto
            {
                Summary = analysisJson
            };

            conversation.SpaceAnalysisJson = JsonSerializer.Serialize(analysis);
            conversation.Status = AIConversationStatus.Consulting;

            await _repo.AddMessageAsync(new AIMessage
            {
                ConversationId = conversation.Id,
                Role = AIMessageRole.User,
                MessageType = AIMessageType.Image,
                Content = "Đã upload ảnh không gian",
                ImageUrl = imageUrl
            }, ct);

            var assistantText = $"Mình đã xem qua không gian của bạn!\n\n" +
                $"📐 {analysis.EstimatedArea}\n" +
                $"🎨 {analysis.WallColors}\n" +
                $"💡 {analysis.LightingNotes}\n\n" +
                $"{analysis.SetupRecommendation}";

            await _repo.AddMessageAsync(new AIMessage
            {
                ConversationId = conversation.Id,
                Role = AIMessageRole.Assistant,
                MessageType = AIMessageType.SpaceAnalysis,
                Content = assistantText,
                MetadataJson = JsonSerializer.Serialize(analysis)
            }, ct);

            await _repo.UpdateConversationAsync(conversation, ct);
            await _quota.RecordUsageAsync(userId, AIUsageType.ImageAnalysis, ct: ct);

            return new AIAnalyzeImageResponseDto
            {
                ConversationId = conversation.Id,
                ImageUrl = imageUrl,
                Analysis = analysis,
                AssistantMessage = assistantText
            };
        }

        public async Task<AIGenerateConceptResponseDto> GenerateConceptAsync(
            Guid userId, AIGenerateConceptRequestDto dto, CancellationToken ct = default)
        {
            await _quota.EnsureQuotaAsync(userId, AIUsageType.ConceptGenerate, ct);

            var conversation = await _repo.GetConversationWithMessagesAsync(dto.ConversationId, userId, ct)
                ?? throw new InvalidOperationException("Cuộc hội thoại không tồn tại.");

            var cacheKey = $"concept:{conversation.Id}:{conversation.UpdatedAt.Ticks}:{dto.Regenerate}";
            if (!dto.Regenerate && _cache.TryGetValue(cacheKey, out ConceptProposalDto? cached) && cached != null)
            {
                return new AIGenerateConceptResponseDto
                {
                    ConversationId = conversation.Id,
                    MessageId = Guid.Empty,
                    Concept = cached
                };
            }

            var contextPrompt = BuildConceptContext(conversation);
            var raw = await _gemini.GenerateContentAsync(
                AIPrompts.ConceptGenerationSystem,
                new[] { new GeminiContentPart { Text = contextPrompt } },
                jsonMode: true,
                ct: ct);

            var concept = AIResponseParser.ParseJson<ConceptProposalDto>(raw) ?? new ConceptProposalDto
            {
                ConceptName = "Concept Bloomy",
                Description = raw,
                EstimatedBudget = 4500000
            };

            var matches = await MatchPortfolioAsync(concept);
            concept.PortfolioMatches = matches;
            concept.UsedPortfolioFallback = true;
            concept.PreviewImageUrl ??= matches.FirstOrDefault()?.CoverImageUrl;

            conversation.Status = AIConversationStatus.ConceptGenerated;
            await _repo.UpdateConversationAsync(conversation, ct);

            var msg = await _repo.AddMessageAsync(new AIMessage
            {
                ConversationId = conversation.Id,
                Role = AIMessageRole.Assistant,
                MessageType = AIMessageType.Concept,
                Content = $"Concept: {concept.ConceptName}",
                MetadataJson = JsonSerializer.Serialize(concept)
            }, ct);

            _cache.Set(cacheKey, concept, TimeSpan.FromMinutes(_settings.ConceptCacheMinutes));
            await _quota.RecordUsageAsync(userId, AIUsageType.ConceptGenerate, ct: ct);

            return new AIGenerateConceptResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = msg.Id,
                Concept = concept
            };
        }

        public async Task<List<AIConversationHistoryDto>> GetHistoryAsync(
            Guid userId, Guid? conversationId, CancellationToken ct = default)
        {
            if (conversationId.HasValue)
            {
                var conv = await _repo.GetConversationWithMessagesAsync(conversationId.Value, userId, ct);
                return conv == null ? new List<AIConversationHistoryDto>() : new List<AIConversationHistoryDto> { MapConversation(conv) };
            }

            var list = await _repo.GetUserConversationsAsync(userId, 20, ct);
            return list.Select(MapConversation).ToList();
        }

        public async Task<SaveAIConceptResponseDto> SaveConceptAsync(
            Guid userId, SaveAIConceptRequestDto dto, CancellationToken ct = default)
        {
            var conceptData = dto.ConceptData ?? new ConceptProposalDto();
            var entity = new SavedConcept
            {
                UserId = userId,
                ConversationId = dto.ConversationId,
                Name = dto.Name ?? conceptData.ConceptName ?? "Concept Bloomy",
                Description = dto.Description ?? conceptData.Description ?? "",
                ToneColor = dto.ToneColor ?? conceptData.ToneColor ?? "",
                Style = dto.Style ?? conceptData.Style ?? "",
                EstimatedBudget = dto.EstimatedBudget > 0 ? dto.EstimatedBudget : conceptData.EstimatedBudget,
                PreviewImageUrl = dto.PreviewImageUrl ?? conceptData.PreviewImageUrl,
                ConceptDataJson = JsonSerializer.Serialize(conceptData),
                MatchedPortfolioIdsJson = dto.MatchedPortfolioIds != null
                    ? JsonSerializer.Serialize(dto.MatchedPortfolioIds)
                    : JsonSerializer.Serialize(conceptData.PortfolioMatches.Select(p => p.Id)),
                IsAiGenerated = true
            };

            var saved = await _repo.SaveConceptAsync(entity, ct);
            return new SaveAIConceptResponseDto
            {
                Id = saved.Id,
                Name = saved.Name,
                EstimatedBudget = saved.EstimatedBudget,
                PreviewImageUrl = saved.PreviewImageUrl
            };
        }

        public async Task<List<SavedConceptListItemDto>> GetSavedConceptsAsync(Guid userId, CancellationToken ct = default)
        {
            var list = await _repo.GetSavedConceptsAsync(userId, ct);
            return list.Select(c => new SavedConceptListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ToneColor = c.ToneColor,
                Style = c.Style,
                EstimatedBudget = c.EstimatedBudget,
                PreviewImageUrl = c.PreviewImageUrl,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task DeleteSavedConceptAsync(Guid userId, Guid conceptId, CancellationToken ct = default)
        {
            var concept = await _repo.GetSavedConceptAsync(conceptId, userId, ct);
            if (concept == null)
                throw new InvalidOperationException("Concept không tồn tại.");
            await _repo.DeleteSavedConceptAsync(conceptId, ct);
        }

        private async Task<AIConversation> GetOrCreateConversationAsync(
            Guid userId, Guid? conversationId, CancellationToken ct)
        {
            if (conversationId.HasValue)
            {
                var existing = await _repo.GetConversationWithMessagesAsync(conversationId.Value, userId, ct);
                if (existing != null) return existing;
            }

            return await _repo.CreateConversationAsync(new AIConversation
            {
                UserId = userId,
                Title = "Tư vấn decor mới",
                Status = AIConversationStatus.Consulting
            }, ct);
        }

        private static List<GeminiContentPart> BuildChatHistory(AIConversation conversation)
        {
            return conversation.Messages
                .Where(m => m.MessageType is AIMessageType.Text or AIMessageType.SpaceAnalysis)
                .Where(m => m.Role == AIMessageRole.User || !string.IsNullOrWhiteSpace(m.Content))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new GeminiContentPart
                {
                    Role = m.Role == AIMessageRole.Assistant ? "assistant" : "user",
                    Text = m.Content
                })
                .ToList();
        }

        private async Task UpdateConversationFromMetadata(AIConversation conversation, ChatMetadataPayload? metadata, CancellationToken ct)
        {
            if (metadata == null) return;

            if (metadata.GatheredRequirements != null)
            {
                var merged = AIResponseParser.MergeRequirements(
                    conversation.GatheredRequirementsJson,
                    metadata.GatheredRequirements);
                conversation.GatheredRequirementsJson = JsonSerializer.Serialize(merged);
            }

            if (!string.IsNullOrWhiteSpace(metadata.SuggestedTitle))
                conversation.Title = metadata.SuggestedTitle!;

            if (metadata.IsReadyForConcept)
                conversation.Status = AIConversationStatus.ReadyForConcept;

            await _repo.UpdateConversationAsync(conversation, ct);
        }

        private static ChatMetadataPayload? ParseChatMetadata(JsonDocument? doc)
        {
            if (doc == null) return null;
            try
            {
                return JsonSerializer.Deserialize<ChatMetadataPayload>(
                    doc.RootElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }

        private string BuildConceptContext(AIConversation conversation)
        {
            var messages = string.Join("\n", conversation.Messages
                .Where(m => m.Role != AIMessageRole.System)
                .Select(m => $"{m.Role}: {m.Content}"));

            return $"""
                YÊU CẦU ĐÃ THU THẬP:
                {conversation.GatheredRequirementsJson}

                PHÂN TÍCH KHÔNG GIAN:
                {conversation.SpaceAnalysisJson ?? "Chưa có ảnh không gian"}

                LỊCH SỬ TRÒ CHUYỆN:
                {messages}

                Hãy tạo concept decor phù hợp.
                """;
        }

        private async Task<List<PortfolioMatchDto>> MatchPortfolioAsync(ConceptProposalDto concept)
        {
            var items = await _repo.GetPortfolioItemsForMatchingAsync();
            var tone = concept.ToneColor.ToLowerInvariant();
            var style = concept.Style.ToLowerInvariant();
            var eventType = concept.EventType.ToLowerInvariant();

            var scored = items.Select(item =>
            {
                var score = 0;
                var itemTone = (item.ToneColor ?? "").ToLowerInvariant();
                var itemStyle = (item.Style ?? "").ToLowerInvariant();
                var itemTags = (item.Tags ?? "").ToLowerInvariant();
                var itemTitle = item.Title.ToLowerInvariant();
                var itemDesc = item.Description.ToLowerInvariant();

                if (!string.IsNullOrEmpty(tone) && (itemTone.Contains(tone) || itemTags.Contains(tone) || itemTitle.Contains(tone)))
                    score += 30;
                if (!string.IsNullOrEmpty(style) && (itemStyle.Contains(style) || itemTags.Contains(style)))
                    score += 25;
                if (!string.IsNullOrEmpty(eventType) && (itemTags.Contains(eventType) || itemTitle.Contains(eventType) ||
                    (item.EventType?.Name ?? "").ToLowerInvariant().Contains(eventType)))
                    score += 20;

                foreach (var word in tone.Split(' ', ',', '&', '-').Where(w => w.Length > 2))
                {
                    if (itemTags.Contains(word) || itemTitle.Contains(word)) score += 5;
                }

                var cover = item.Images.OrderBy(i => i.OrderIndex).FirstOrDefault()?.ImageUrl ?? "";

                return new PortfolioMatchDto
                {
                    Id = item.Id,
                    Title = item.Title,
                    CoverImageUrl = cover,
                    ToneColor = item.ToneColor,
                    Style = item.Style,
                    Price = item.Price,
                    MatchScore = score
                };
            })
            .Where(x => x.MatchScore > 0)
            .OrderByDescending(x => x.MatchScore)
            .Take(4)
            .ToList();

            if (scored.Count == 0)
            {
                scored = items.Take(3).Select(item => new PortfolioMatchDto
                {
                    Id = item.Id,
                    Title = item.Title,
                    CoverImageUrl = item.Images.OrderBy(i => i.OrderIndex).FirstOrDefault()?.ImageUrl ?? "",
                    ToneColor = item.ToneColor,
                    Style = item.Style,
                    Price = item.Price,
                    MatchScore = 1
                }).ToList();
            }

            return scored;
        }

        private void ValidateImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new InvalidOperationException("Vui lòng chọn ảnh hợp lệ.");

            if (image.Length > _settings.MaxImageUploadBytes)
                throw new InvalidOperationException($"Ảnh không được vượt quá {_settings.MaxImageUploadBytes / 1024 / 1024}MB.");

            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!_settings.AllowedImageExtensions.Contains(ext))
                throw new InvalidOperationException("Chỉ chấp nhận ảnh JPG, PNG, WEBP.");
        }

        private async Task<string> SaveUploadedImageAsync(IFormFile image, CancellationToken ct)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "ai-spaces");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName).ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = File.Create(filePath);
            await image.CopyToAsync(stream, ct);

            return $"/uploads/ai-spaces/{fileName}";
        }

        private static AIConversationHistoryDto MapConversation(AIConversation conv)
        {
            ConceptProposalDto? latestConcept = null;
            var messages = conv.Messages.OrderBy(m => m.CreatedAt).Select(m =>
            {
                ConceptProposalDto? concept = null;
                SpaceAnalysisDto? analysis = null;

                if (m.MessageType == AIMessageType.Concept && !string.IsNullOrEmpty(m.MetadataJson))
                    concept = AIResponseParser.ParseJson<ConceptProposalDto>(m.MetadataJson);
                if (m.MessageType == AIMessageType.SpaceAnalysis && !string.IsNullOrEmpty(m.MetadataJson))
                    analysis = AIResponseParser.ParseJson<SpaceAnalysisDto>(m.MetadataJson);

                if (concept != null) latestConcept = concept;

                return new AIMessageDto
                {
                    Id = m.Id,
                    Role = m.Role.ToString().ToLowerInvariant(),
                    MessageType = m.MessageType.ToString().ToLowerInvariant(),
                    Content = m.Content,
                    ImageUrl = m.ImageUrl,
                    Concept = concept,
                    SpaceAnalysis = analysis,
                    CreatedAt = m.CreatedAt
                };
            }).ToList();

            return new AIConversationHistoryDto
            {
                Id = conv.Id,
                Title = conv.Title,
                Status = conv.Status.ToString(),
                CreatedAt = conv.CreatedAt,
                UpdatedAt = conv.UpdatedAt,
                MessageCount = messages.Count,
                Messages = messages,
                LatestConcept = latestConcept
            };
        }
    }
}
