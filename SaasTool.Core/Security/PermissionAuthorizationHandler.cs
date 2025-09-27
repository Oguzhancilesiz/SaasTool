using Microsoft.AspNetCore.Authorization;
using SaasTool.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Core.Security
{
    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ICurrentUser _current;
        public PermissionAuthorizationHandler(ICurrentUser current) => _current = current;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Burayı DB'den permission kontrolüyle genişletebilirsin.
            // Şimdilik claim veya role üzerinden basit örnek:
            if (_current.IsAuthenticated && context.User.HasClaim("permission", requirement.Permission))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

}
