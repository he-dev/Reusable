using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public class HttpRequest : Request
    {
        public ProductInfoHeaderValue? UserAgent { get; set; }

        public List<Action<HttpRequestHeaders>> HeaderActions { get; } = new List<Action<HttpRequestHeaders>>();

        //public MediaTypeFormatter? RequestFormatter { get; set; }

        public string ContentType { get; set; } = default!;

        public class Json : HttpRequest
        {
            public Json()
            {
                HeaderActions.Add(headers => headers.AcceptJson());
                ContentType = "application/json";
            }
        }
    }
}