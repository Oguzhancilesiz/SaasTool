using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Dashboard
{
    public sealed class BreakdownItemDto
    {
        public string Key { get; init; } = null!;
        public decimal Value { get; init; }
    }

}
