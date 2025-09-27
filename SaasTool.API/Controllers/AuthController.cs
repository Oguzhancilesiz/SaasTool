using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaasTool.DTO.Security;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;

namespace SaasTool.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userMgr;
        private readonly RoleManager<AppRole> _roleMgr;
        private readonly ITokenService _tokenSvc;

        public AuthController(UserManager<AppUser> userMgr, RoleManager<AppRole> roleMgr, ITokenService tokenSvc)
        {
            _userMgr = userMgr;
            _roleMgr = roleMgr;
            _tokenSvc = tokenSvc;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userMgr.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            // Varsayılan USER rolü (yoksa oluştur)
            const string userRole = "User";
            if (!await _roleMgr.RoleExistsAsync(userRole))
                await _roleMgr.CreateAsync(new AppRole { Id = Guid.NewGuid(), Name = userRole });

            await _userMgr.AddToRoleAsync(user, userRole);
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            var user = await _userMgr.FindByEmailAsync(dto.Email);
            if (user is null) return Unauthorized();

            if (!await _userMgr.CheckPasswordAsync(user, dto.Password))
                return Unauthorized();

            var roles = await _userMgr.GetRolesAsync(user);
            var (token, exp) = _tokenSvc.Create(user, roles);
            return Ok(new TokenResponse { AccessToken = token, ExpiresAtUtc = exp });
        }
    }
}
