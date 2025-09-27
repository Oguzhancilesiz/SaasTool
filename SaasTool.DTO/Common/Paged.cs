using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Common
{
    public sealed class PagedRequest
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? Search { get; init; }   // <- PlanService şikayet ediyordu, artık var.
    }

    public sealed class PagedResponse<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int Total { get; }
        public int Page { get; }
        public int PageSize { get; }

        public PagedResponse(IReadOnlyList<T> items, int total, int page, int pageSize)
        {
            Items = items;
            Total = total;
            Page = page;
            PageSize = pageSize;
        }
    }
}
