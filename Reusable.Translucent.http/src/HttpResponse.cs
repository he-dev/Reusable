using JetBrains.Annotations;

namespace Reusable.Translucent
{
    [PublicAPI]
    public class HttpResponse : Response
    {
        public int HttpStatusCode { get; set; }

        public HttpStatusCodeClass HttpStatusCodeClass => HttpStatusCodeMapper.MapStatusCode(HttpStatusCode);

        public string? ContentType { get; set; }
    }
}