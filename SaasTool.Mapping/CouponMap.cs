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
    public class CouponMap : BaseMap<Coupon>
    {
        public override void Configure(EntityTypeBuilder<Coupon> b)
        {
            base.Configure(b);
            b.ToTable("Coupons");
            b.Property(x => x.Code).HasMaxLength(80).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        }
    }

}
