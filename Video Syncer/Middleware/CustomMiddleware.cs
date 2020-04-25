using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Middleware
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next)
        {
            
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            System.Diagnostics.Debug.WriteLine("custom middleware is called");

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
