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
    public class PlanMap : BaseMap<Plan>
    {
        public override void Configure(EntityTypeBuilder<Plan> b)
        {
            base.Configure(b);
            b.ToTable("Plans");
            b.Property(x => x.Code).HasMaxLength(100).IsRequired();
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.HasIndex(x => x.Code).IsUnique();
        }
    }

}
