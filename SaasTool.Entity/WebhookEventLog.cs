using SaasTool.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class WebhookEventLog : BaseEntity
    {
        public PaymentProvider Provider { get; set; }
        public string EventType { get; set; } = null!;
        public string PayloadJson { get; set; } = null!;
        public bool IsProcessed { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessNote { get; set; }
    }

}
