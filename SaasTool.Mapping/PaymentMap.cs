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
    public class PaymentMap : BaseMap<Payment>
    {
        public override void Configure(EntityTypeBuilder<Payment> b)
        {
            base.Configure(b);
            b.ToTable("Payments");
            b.HasOne(x => x.Invoice).WithMany(x => x.Payments).HasForeignKey(x => x.InvoiceId);
        }
    }

}
