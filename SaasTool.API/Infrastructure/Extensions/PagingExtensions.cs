using SaasTool.DTO.Common;

namespace SaasTool.API.Infrastructure.Extensions
{
    public static class PagingExtensions
    {
        public static PagedRequest Normalize(this PagedRequest req, int maxPageSize = 200)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var size = req.PageSize <= 0 ? 10 : req.PageSize;
            if (size > maxPageSize) size = maxPageSize;
            return new PagedRequest { Page = page, PageSize = size, Search = req.Search };
        }
    }
}
