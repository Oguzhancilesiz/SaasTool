using SaasTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface ITokenService
    {
        (string token, DateTime expiresUtc) Create(AppUser user, IEnumerable<string> roles);
    }
}
