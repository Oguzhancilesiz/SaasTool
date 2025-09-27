using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Security
{
    public sealed class RegisterDto
    {
        public string Email { get; init; } = default!;
        public string Password { get; init; } = default!;
    }

    public sealed class LoginDto
    {
        public string Email { get; init; } = default!;
        public string Password { get; init; } = default!;
    }

    public sealed class TokenResponse
    {
        public string AccessToken { get; init; } = default!;
        public DateTime ExpiresAtUtc { get; init; }
    }

}
