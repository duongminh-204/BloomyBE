using Bloomy.Models.Enums;

namespace Bloomy.Models
{
    public class AIUsage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }
        public User? User { get; set; }

        public AIUsageType UsageType { get; set; }

        public int RequestCount { get; set; } = 1;

        public int? TokensUsed { get; set; }

        /// <summary>UTC date bucket for daily quota (date only, no time)</summary>
        public DateTime UsageDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
