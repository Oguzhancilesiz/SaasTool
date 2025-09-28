using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Dashboard
{
    public sealed class SeriesPointDto
    {
        public DateTime T { get; init; } // UTC
        public decimal V { get; init; }
    }

}
