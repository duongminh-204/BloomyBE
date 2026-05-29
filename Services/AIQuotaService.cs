using Bloomy.Models.Enums;
using BloomyBE.Configuration;
using BloomyBE.DTOs.AI;
using BloomyBE.Repositories.Interfaces;
using BloomyBE.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BloomyBE.Services
{
    public interface IAIQuotaService
    {
        Task EnsureQuotaAsync(Guid userId, AIUsageType type, CancellationToken ct = default);
        Task RecordUsageAsync(Guid userId, AIUsageType type, int tokensUsed = 0, CancellationToken ct = default);
        Task<AIQuotaStatusDto> GetQuotaStatusAsync(Guid userId, CancellationToken ct = default);
    }

    public class AIQuotaService : IAIQuotaService
    {
        private readonly IAIRepository _repo;
        private readonly GeminiSettings _settings;

        public AIQuotaService(IAIRepository repo, IOptions<GeminiSettings> settings)
        {
            _repo = repo;
            _settings = settings.Value;
        }

        public async Task EnsureQuotaAsync(Guid userId, AIUsageType type, CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            var used = await _repo.GetDailyUsageCountAsync(userId, type, today, ct);
            var limit = GetLimit(type);

            if (used >= limit)
                throw new InvalidOperationException(GetQuotaMessage(type));
        }

        public async Task RecordUsageAsync(Guid userId, AIUsageType type, int tokensUsed = 0, CancellationToken ct = default)
        {
            await _repo.RecordUsageAsync(new Models.AIUsage
            {
                UserId = userId,
                UsageType = type,
                RequestCount = 1,
                TokensUsed = tokensUsed > 0 ? tokensUsed : null,
                UsageDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            }, ct);
        }

        public async Task<AIQuotaStatusDto> GetQuotaStatusAsync(Guid userId, CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            var chatUsed = await _repo.GetDailyUsageCountAsync(userId, AIUsageType.Chat, today, ct);
            var imageUsed = await _repo.GetDailyUsageCountAsync(userId, AIUsageType.ImageAnalysis, today, ct);
            var conceptUsed = await _repo.GetDailyUsageCountAsync(userId, AIUsageType.ConceptGenerate, today, ct);
            var imageGenUsed = await _repo.GetDailyUsageCountAsync(userId, AIUsageType.ImageGenerate, today, ct);

            return new AIQuotaStatusDto
            {
                ChatRemaining = Math.Max(0, _settings.MaxDailyChatRequests - chatUsed),
                ImageAnalysisRemaining = Math.Max(0, _settings.MaxDailyImageAnalysis - imageUsed),
                ConceptGenerateRemaining = Math.Max(0, _settings.MaxDailyConceptGenerations - conceptUsed),
                ImageGenerateRemaining = Math.Max(0, _settings.MaxDailyImageGenerations - imageGenUsed)
            };
        }

        private int GetLimit(AIUsageType type) => type switch
        {
            AIUsageType.Chat => _settings.MaxDailyChatRequests,
            AIUsageType.ImageAnalysis => _settings.MaxDailyImageAnalysis,
            AIUsageType.ConceptGenerate => _settings.MaxDailyConceptGenerations,
            AIUsageType.ImageGenerate => _settings.MaxDailyImageGenerations,
            _ => 10
        };

        private static string GetQuotaMessage(AIUsageType type) => type switch
        {
            AIUsageType.Chat => "Bạn đã hết lượt chat AI hôm nay. Vui lòng quay lại vào ngày mai.",
            AIUsageType.ImageAnalysis => "Bạn đã hết lượt phân tích ảnh hôm nay.",
            AIUsageType.ConceptGenerate => "Bạn đã hết lượt tạo concept hôm nay.",
            AIUsageType.ImageGenerate => "Bạn đã hết lượt tạo ảnh preview hôm nay.",
            _ => "Đã vượt giới hạn sử dụng AI."
        };
    }
}
