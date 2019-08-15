﻿using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Translucent
{
    [PublicAPI]
    public interface IResourceController : IDisposable
    {
        [NotNull]
        IImmutableContainer Properties { get; }
    }

    public delegate Task<Response> InvokeCallback(Request request);

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceController : IResourceController
    {
        protected ResourceController([NotNull] IImmutableContainer properties)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            if (properties.GetItemOrDefault(Schemes) is var schemes && (schemes is null || !schemes.Any()))
            {
                throw new ArgumentException
                (
                    paramName: nameof(properties),
                    message: $"{GetType().ToPrettyString().ToSoftString()} must specify at least one scheme."
                );
            }

            Properties = properties.UpdateItem(Tags, tags => tags.Add(GetType().ToPrettyString().ToSoftString()));
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            //builder.DisplayEnumerable(p => p.Properties.Tags(), x => x.ToString());
            //builder.DisplayEnumerable(p => p.Properties.GetSchemes(), x => x.ToString());
            //builder.DisplayValues(p => Names);
            //builder.DisplayValue(x => x.Schemes);
        });

        public virtual IImmutableContainer Properties { get; }


        protected Response OK(Stream body, MimeType contentType, IImmutableContainer metadata = default) => new Response.OK
        {
            Body = body,
            ContentType = contentType,
            Metadata = metadata
        };
        
        protected Response OK(Stream body, IImmutableContainer metadata = default) => new Response.OK
        {
            Body = body,
            Metadata = metadata
        };

        protected Response OK() => new Response.OK();

        protected Response NotFound() => new Response.NotFound();

        // Can be overriden when derived.
        public virtual void Dispose() { }

        #region Properties

        private static readonly From<ResourceController> This;

        public static readonly Selector<IImmutableSet<SoftString>> Schemes = This.Select(() => Schemes);

        public static readonly Selector<IImmutableSet<SoftString>> Tags = This.Select(() => Tags);

        public static readonly Selector<bool> SupportsRelativeUri = This.Select(() => SupportsRelativeUri);

        #endregion
    }

    public static class ResourceProviderExtensions
    {
        public static bool SupportsRelativeUri(this IResourceController resourceController)
        {
            return resourceController.Properties.GetItemOrDefault(ResourceController.SupportsRelativeUri);
        }
    }

    public delegate Task<Stream> CreateStreamCallback();

    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class Request
    {
        [NotNull]
        public UriString Uri { get; set; } = new UriString($"{UriSchemes.Custom.IOnymous}:///");

        [NotNull]
        public RequestMethod Method { get; set; } = RequestMethod.None;

        [NotNull]
        public IImmutableContainer Metadata { get; set; } = ImmutableContainer.Empty;

        [CanBeNull]
        public object Body { get; set; }

        [CanBeNull]
        public CreateStreamCallback CreateBodyStreamCallback { get; set; }

        public MimeType ContentType { get; set; }

        #region Methods

        public class Get : Request
        {
            public Get(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Get;
            }
        }

        public class Post : Request
        {
            public Post(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Post;
            }
        }

        public class Put : Request
        {
            public Put(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Put;
            }
        }

        public class Delete : Request
        {
            public Delete(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Delete;
            }
        }

        #endregion

        #region Properties

        private static readonly From<Request> This;

        public static readonly Selector<MimeType> Accept = This.Select(() => Accept);

        public static readonly Selector<Encoding> Encoding = This.Select(() => Encoding);

        #endregion
    }

    public static class Body
    {
        public static readonly object Null = new object();
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(RequestProperty))]
    public class RequestProperty : SelectorBuilder<RequestProperty>
    {
        public static readonly Selector<CancellationToken> CancellationToken = Select(() => CancellationToken);
    }

    public static class RequestExtensions
    {
        public static Task<Stream> CreateBodyStreamAsync(this Request request)
        {
            switch (request.Body)
            {
                case Stream stream: return stream.ToTask();
                case string str: return str.ToStream().ToTask();
                default:
                    return request.CreateBodyStreamCallback?.Invoke() ??
                           throw new InvalidOperationException($"Cannot create body stream because {nameof(Request.CreateBodyStreamCallback)} is not set.");
            }
        }

        public static Request SetCreateBodyStream(this Request request, CreateStreamCallback createBodyStream)
        {
            request.CreateBodyStreamCallback = createBodyStream;
            return request;
        }
    }

    public class RequestMethod : Option<RequestMethod>
    {
        public RequestMethod(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        public static readonly RequestMethod Get = CreateWithCallerName();

        public static readonly RequestMethod Post = CreateWithCallerName();

        public static readonly RequestMethod Put = CreateWithCallerName();

        public static readonly RequestMethod Delete = CreateWithCallerName();
    }

    public static class RequestHelper
    {
        public static string FormatMethodName(Request request) => request.Method.Name.ToString().ToUpper();
    }

    public static class ElementOrder
    {
        public const int Preserve = -1; // Less than zero - x is less than y.
        public const int Ignore = 0; // Zero - x equals y.
        public const int Reorder = 1; // Greater than zero - x is greater than y.
    }

    public static class ComparerHelper
    {
        public static bool TryCompareReference<T>(T x, T y, out int referenceOrder)
        {
            if (ReferenceEquals(x, y))
            {
                referenceOrder = ElementOrder.Ignore;
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                referenceOrder = ElementOrder.Preserve;
                return true;
            }

            if (ReferenceEquals(y, null))
            {
                referenceOrder = ElementOrder.Reorder;
                return true;
            }

            referenceOrder = ElementOrder.Ignore;
            return false;
        }
    }

    public abstract class ResourceActionAttribute : Attribute
    {
        protected ResourceActionAttribute(RequestMethod method) => Method = method;

        public RequestMethod Method { get; }
    }

    public class ResourceGetAttribute : ResourceActionAttribute
    {
        public ResourceGetAttribute() : base(RequestMethod.Get) { }
    }

    public class ResourcePostAttribute : ResourceActionAttribute
    {
        public ResourcePostAttribute() : base(RequestMethod.Post) { }
    }

    public class ResourcePutAttribute : ResourceActionAttribute
    {
        public ResourcePutAttribute() : base(RequestMethod.Put) { }
    }

    public class ResourceDeleteAttribute : ResourceActionAttribute
    {
        public ResourceDeleteAttribute() : base(RequestMethod.Delete) { }
    }
}