using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Mapping
{
    public class WebhookEventLogMap : BaseMap<WebhookEventLog>
    {
        public override void Configure(EntityTypeBuilder<WebhookEventLog> b)
        {
            base.Configure(b);
            b.ToTable("WebhookEventLogs");
            b.Property(x => x.EventType).HasMaxLength(150).IsRequired();
            b.Property(x => x.PayloadJson).IsRequired();
        }
    }

}
