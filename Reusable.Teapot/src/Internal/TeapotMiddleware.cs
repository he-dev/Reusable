﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Reusable.Teapot.Internal
{
    internal class TeapotMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ConcurrentDictionary<PathString, List<RequestInfo>> _requests;

        public TeapotMiddleware(RequestDelegate next, ConcurrentDictionary<PathString, List<RequestInfo>> requests)
        {
            _next = next;
            _requests = requests;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // We'll need this later so don't dispose.
                var memory = new MemoryStream();
                {
                    // It needs to be copied because otherwise it'll get disposed.
                    await context.Request.Body.CopyToAsync(memory);

                    var request = new RequestInfo
                    {
                        Path = context.Request.Path,
                        ContentLength = context.Request.ContentLength,
                        // There is no copy-constructor.
                        Headers = new HeaderDictionary(context.Request.Headers.ToDictionary(x => x.Key, x => x.Value)),
                        BodyStreamCopy = memory
                    };

                    _requests.AddOrUpdate
                    (
                        context.Request.Path, 
                        path => new List<RequestInfo> { request }, 
                        (path, requests) =>
                        {
                            requests.Add(request);
                            return requests;
                        });

                    await _next(context);
                    context.Response.StatusCode = StatusCodes.Status418ImATeapot;
                }

            }
            catch (Exception inner)
            {
                // Not sure what to do... throw or not?
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                //throw;
            }
        }
    }
}