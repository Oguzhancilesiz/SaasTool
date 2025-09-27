using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Core.Abstracts
{
    public interface ITenantContext
    {
        public Guid? Id { get; set; }
        public string? Code { get; set; }
    }
}
