﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncd.Helpers;

namespace Syncd.Middleware
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            SessionHelper.SetupSession(context);

            // Call the next delegate/middleware in the pipeline
            return _next(context);
        }
    }
}
