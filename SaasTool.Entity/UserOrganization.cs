using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class UserOrganization : BaseEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public Guid UserId { get; set; } // AppUser Id
        public string Role { get; set; } = "Owner";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

}
