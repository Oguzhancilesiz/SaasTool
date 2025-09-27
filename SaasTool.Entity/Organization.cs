using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Slug { get; set; }

        public ICollection<UserOrganization> Users { get; set; } = new List<UserOrganization>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
        public ICollection<AppUserProfile> UserProfiles { get; set; } = new List<AppUserProfile>();
    }
}
