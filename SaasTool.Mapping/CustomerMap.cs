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
    public class CustomerMap : BaseMap<Customer>
    {
        public override void Configure(EntityTypeBuilder<Customer> b)
        {
            base.Configure(b);
            b.ToTable("Customers");
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Email).HasMaxLength(200).IsRequired();
            b.Property(x => x.TaxNumber).HasMaxLength(50);
            b.HasIndex(x => new { x.OrganizationId, x.Email });
            b.HasOne(x => x.Organization).WithMany(x => x.Customers).HasForeignKey(x => x.OrganizationId);
        }
    }

}
