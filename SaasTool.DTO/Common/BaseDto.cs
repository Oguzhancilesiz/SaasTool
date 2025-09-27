using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Common
{
    public abstract record BaseDto
    {
        public Guid Id { get; init; }
    }

    public abstract record AuditedDto : BaseDto
    {
        public DateTime CreatedDate { get; init; }
        public DateTime ModifiedDate { get; init; }
        public int AutoID { get; init; }
        public int Status { get; init; } // Core.Enums.Status int olarak dışarı veriyoruz
    }

}
