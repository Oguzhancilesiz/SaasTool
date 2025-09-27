using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class ApiKey : BaseEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public Guid AppId { get; set; }
        public App App { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string KeyHash { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }

}
