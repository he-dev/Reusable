﻿using System.Threading;
using System.Threading.Tasks;
using Reusable.sdk.Http;
using Reusable.sdk.Http.Formatting;

namespace Reusable.sdk.Mailr
{
    public static class MailrClientExtensions
    {
        public static IResource<IMailrClient> Resource(this IMailrClient client, params string[] path) => client.Resource<IMailrClient>(path);

        /// <summary>
        /// Sends the specified email and returns its body as a response.
        /// </summary>
        public static Task<string> SendAsync<TBody>(this IResource<IMailrClient> resource, Email<TBody> email, CancellationToken cancellationToken = default)
        {
            resource.Configure(context =>
            {
                context.Body = email;
                context.ResponseFormatters.Add(new TextMediaTypeFormatter());
                context.RequestHeadersActions.Add(headers => headers.AcceptHtml());
            });
            
            return resource.PostAsync<string>(cancellationToken);
        }
    }
}