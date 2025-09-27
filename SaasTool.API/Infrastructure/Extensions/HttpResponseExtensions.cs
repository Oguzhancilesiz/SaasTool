namespace SaasTool.API.Infrastructure.Extensions
{
    public static class HttpResponseExtensions
    {
        public static bool TryShortCircuitWithEtag(this HttpRequest req, HttpResponse res, string etag)
        {
            res.Headers.ETag = etag;
            if (req.Headers.TryGetValue("If-None-Match", out var inm) && inm.ToString() == etag)
            {
                res.StatusCode = StatusCodes.Status304NotModified;
                return true;
            }
            return false;
        }
    }

}
