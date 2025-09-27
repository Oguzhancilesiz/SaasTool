using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Core.Abstracts
{
    public interface ICurrentUser
    {
        public Guid? UserId { get; }
        public string? Email { get; }
        public bool IsAuthenticated { get; }
        public bool IsInRole(string role);
    }

}
