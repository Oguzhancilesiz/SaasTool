using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Core.Abstracts
{
    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
