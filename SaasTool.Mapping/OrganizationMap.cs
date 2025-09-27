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
    public class OrganizationMap : BaseMap<Organization>
    {
        public override void Configure(EntityTypeBuilder<Organization> b)
        {
            base.Configure(b);
            b.ToTable("Organizations");
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Slug).HasMaxLength(120);
            b.HasIndex(x => x.Slug).IsUnique(false);
        }
    }

}
