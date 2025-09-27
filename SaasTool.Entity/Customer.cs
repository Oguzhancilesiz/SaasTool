using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class Customer : BaseEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? TaxNumber { get; set; }
        public string? BillingAddress { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }

}
