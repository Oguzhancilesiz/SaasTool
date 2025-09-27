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
    public class UserOrganizationMap : BaseMap<UserOrganization>
    {
        public override void Configure(EntityTypeBuilder<UserOrganization> b)
        {
            base.Configure(b);
            b.ToTable("UserOrganizations");
            b.Property(x => x.Role).HasMaxLength(80).IsRequired();
            b.HasIndex(x => new { x.OrganizationId, x.UserId }).IsUnique();
            b.HasOne(x => x.Organization).WithMany(x => x.Users).HasForeignKey(x => x.OrganizationId);
        }
    }

}
