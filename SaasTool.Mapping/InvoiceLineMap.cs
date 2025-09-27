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
    public class InvoiceLineMap : BaseMap<InvoiceLine>
    {
        public override void Configure(EntityTypeBuilder<InvoiceLine> b)
        {
            base.Configure(b);
            b.ToTable("InvoiceLines");
            b.Property(x => x.Description).HasMaxLength(300).IsRequired();
            b.HasOne(x => x.Invoice).WithMany(x => x.Lines).HasForeignKey(x => x.InvoiceId);
        }
    }

}
