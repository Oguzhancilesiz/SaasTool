namespace SaasTool.DTO.Common
{
    public sealed class PagedRequest
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string? Search { get; init; }

        // Artık extension değil, instance metodu. Service & API her yerden kullanır.
        public PagedRequest Normalize(int maxPageSize = 200)
        {
            var page = Page <= 0 ? 1 : Page;
            var size = PageSize <= 0 ? 10 : PageSize;
            if (size > maxPageSize) size = maxPageSize;

            return new PagedRequest
            {
                Page = page,
                PageSize = size,
                Search = Search
            };
        }
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
