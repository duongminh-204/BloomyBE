using Bloomy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomyBE.Data.Configurations
{
    public class AIConversationConfiguration : IEntityTypeConfiguration<AIConversation>
    {
        public void Configure(EntityTypeBuilder<AIConversation> builder)
        {
            builder.ToTable("AIConversations");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.GatheredRequirementsJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.SpaceAnalysisJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.UploadedSpaceImageUrl).HasMaxLength(500);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.CreatedAt);

            builder.HasOne(x => x.User)
                .WithMany(u => u.AIConversations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class AIMessageConfiguration : IEntityTypeConfiguration<AIMessage>
    {
        public void Configure(EntityTypeBuilder<AIMessage> builder)
        {
            builder.ToTable("AIMessages");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content).HasColumnType("nvarchar(max)");
            builder.Property(x => x.ImageUrl).HasMaxLength(500);
            builder.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");

            builder.HasIndex(x => x.ConversationId);
            builder.HasIndex(x => x.CreatedAt);

            builder.HasOne(x => x.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class SavedConceptConfiguration : IEntityTypeConfiguration<SavedConcept>
    {
        public void Configure(EntityTypeBuilder<SavedConcept> builder)
        {
            builder.ToTable("SavedConcepts");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasColumnType("nvarchar(max)");
            builder.Property(x => x.ToneColor).HasMaxLength(100);
            builder.Property(x => x.Style).HasMaxLength(100);
            builder.Property(x => x.PreviewImageUrl).HasMaxLength(500);
            builder.Property(x => x.ConceptDataJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.MatchedPortfolioIdsJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.EstimatedBudget).HasColumnType("decimal(18,2)");

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.CreatedAt);

            builder.HasOne(x => x.User)
                .WithMany(u => u.SavedConcepts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Conversation)
                .WithMany(c => c.SavedConcepts)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }

    public class AIUsageConfiguration : IEntityTypeConfiguration<AIUsage>
    {
        public void Configure(EntityTypeBuilder<AIUsage> builder)
        {
            builder.ToTable("AIUsages");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.UserId, x.UsageType, x.UsageDate });

            builder.HasOne(x => x.User)
                .WithMany(u => u.AIUsages)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
