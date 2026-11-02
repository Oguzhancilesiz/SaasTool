using SaasTool.DTO.Common;

namespace SaasTool.Tests
{
    public class PagedRequestTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(-5, 1)]
        [InlineData(3, 3)]
        public void Normalize_clamps_page_to_at_least_one(int page, int expected)
        {
            var result = new PagedRequest { Page = page }.Normalize();

            Assert.Equal(expected, result.Page);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(-1, 10)]
        [InlineData(50, 50)]
        [InlineData(500, 200)]
        public void Normalize_defaults_and_caps_page_size(int pageSize, int expected)
        {
            var result = new PagedRequest { PageSize = pageSize }.Normalize();

            Assert.Equal(expected, result.PageSize);
        }

        [Fact]
        public void Normalize_respects_custom_max_page_size()
        {
            var result = new PagedRequest { PageSize = 80 }.Normalize(maxPageSize: 25);

            Assert.Equal(25, result.PageSize);
        }

        [Fact]
        public void Normalize_keeps_search_and_does_not_mutate_original()
        {
            var original = new PagedRequest { Page = 0, PageSize = 0, Search = "acme" };

            var result = original.Normalize();

            Assert.Equal("acme", result.Search);
            Assert.Equal(0, original.Page);
            Assert.Equal(0, original.PageSize);
        }
    }
}
