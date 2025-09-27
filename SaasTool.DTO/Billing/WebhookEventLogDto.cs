using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Billing
{
    public record WebhookEventLogDto : AuditedDto
    {
        public int Provider { get; init; }
        public string EventType { get; init; } = default!;
        public string PayloadJson { get; init; } = default!;
        public bool IsProcessed { get; init; }
        public DateTime? ProcessedAt { get; init; }
        public string? ProcessNote { get; init; }
    }

}
