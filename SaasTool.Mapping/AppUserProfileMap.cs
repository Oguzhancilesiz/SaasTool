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
    public class AppUserProfileMap : BaseMap<AppUserProfile>
    {
        public override void Configure(EntityTypeBuilder<AppUserProfile> b)
        {
            base.Configure(b);
            b.ToTable("AppUserProfiles");
            b.Property(x => x.DisplayName).HasMaxLength(150);
            b.HasIndex(x => new { x.OrganizationId, x.AppId, x.UserId }).IsUnique();
            b.HasOne(x => x.Organization).WithMany(x => x.UserProfiles).HasForeignKey(x => x.OrganizationId);
            b.HasOne(x => x.App).WithMany().HasForeignKey(x => x.AppId);
        }
    }

}
