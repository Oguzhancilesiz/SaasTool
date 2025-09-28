using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Apps
{
    public sealed class AppPlanCreateDto
    {
        public Guid AppId { get; init; }
        public Guid PlanId { get; init; }
        public bool IsEnabled { get; init; } = true;
        public int? DisplayOrder { get; init; }
    }

    public sealed class AppPlanUpdateDto
    {
        public bool IsEnabled { get; init; }
        public int? DisplayOrder { get; init; }
    }

    public sealed class AppPlanDto
    {
        public Guid Id { get; init; }
        public Guid AppId { get; init; }
        public Guid PlanId { get; init; }
        public bool IsEnabled { get; init; }
        public int? DisplayOrder { get; init; }

        // İstersen okunabilirlik için birkaç alan daha:
        public string? AppCode { get; init; }
        public string? PlanCode { get; init; }
        public string? PlanName { get; init; }
    }
}
