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
    // InvoiceMap.cs
    public class InvoiceMap : BaseMap<Invoice>
    {
        public override void Configure(EntityTypeBuilder<Invoice> b)
        {
            base.Configure(b);
            b.ToTable("Invoices");
            b.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.InvoiceNumber).IsUnique();

            // İstersen durum üstünden de index atabilirsin:
            b.HasIndex(x => x.InvoiceState);

            b.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
            b.HasOne(x => x.Customer).WithMany(x => x.Invoices).HasForeignKey(x => x.CustomerId);
        }
    }
}
