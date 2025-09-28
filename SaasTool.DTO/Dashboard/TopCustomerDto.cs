using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Dashboard
{
    public sealed class TopCustomerDto { public Guid CustomerId { get; set; } public string Name { get; set; } = ""; public decimal Total { get; set; } }
}
