using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    // Her app için kullanıcıya özel profil/ayar
    public class AppUserProfile : BaseEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public Guid AppId { get; set; }
        public App App { get; set; } = null!;

        public Guid UserId { get; set; } // AppUser Id
        public string? DisplayName { get; set; }
        public string? PreferencesJson { get; set; }
        public bool IsBlocked { get; set; }
    }
}
