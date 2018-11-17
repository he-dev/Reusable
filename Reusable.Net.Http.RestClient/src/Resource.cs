﻿using System;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Net.Http
{
    public static class RestClientExtensions
    {
        [NotNull]
        public static IResource<TClient> Resource<TClient>([NotNull] this TClient client, params string[] path) where TClient : IRestClient
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            return new Resource<TClient>(client, path);
        }
    }

    public interface IResource<out T>
    {
        [NotNull]
        IResource<T> Configure([NotNull] Action<HttpMethodContext> configureContext);

        Task<TResult> GetAsync<TResult>(CancellationToken cancellationToken = default);

        Task<TResult> PutAsync<TResult>(CancellationToken cancellationToken = default);

        Task<TResult> PostAsync<TResult>(CancellationToken cancellationToken = default);

        Task<TResult> DeleteAsync<TResult>(CancellationToken cancellationToken = default);
    }

    [PublicAPI]
    public class Resource<T> : IResource<T>
    {
        private Func<HttpMethodContext, HttpMethodContext> _configure;

        public Resource([NotNull] IRestClient client, params string[] path)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            PartialUriBuilder = new PartialUriBuilder(path);
        }

        [NotNull]
        private IRestClient Client { get; }

        [NotNull]
        public PartialUriBuilder PartialUriBuilder { get; }

        public IResource<T> Configure(Action<HttpMethodContext> configureContext)
        {
            if (configureContext == null) throw new ArgumentNullException(nameof(configureContext));

            _configure = context =>
            {
                configureContext(context);
                return context;
            };

            return this;
        }

        public Task<TResult> GetAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.InvokeAsync<TResult>(_configure(new HttpMethodContext(HttpMethod.Get, PartialUriBuilder)), cancellationToken);
        }

        public Task<TResult> PutAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.InvokeAsync<TResult>(_configure(new HttpMethodContext(HttpMethod.Put, PartialUriBuilder)), cancellationToken);
        }

        public Task<TResult> PostAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.InvokeAsync<TResult>(_configure(new HttpMethodContext(HttpMethod.Post, PartialUriBuilder)), cancellationToken);
        }

        public Task<TResult> DeleteAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.InvokeAsync<TResult>(_configure(new HttpMethodContext(HttpMethod.Delete, PartialUriBuilder)), cancellationToken);
        }
    }
}