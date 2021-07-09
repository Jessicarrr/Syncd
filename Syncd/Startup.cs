using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Syncd.Middleware;
using System.Diagnostics;
using Microsoft.ApplicationInsights.TraceListener;
using Syncd.logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using Syncd.Models.Users;
using Syncd.Models;
using Syncd.Models.Users.Interface;
using Syncd.Models.Users.Impl;
using Microsoft.AspNetCore.HttpOverrides;

namespace Snycd
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_syncdOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                //options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            Trace.Listeners.Add(new ApplicationInsightsTraceListener());
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddAzureWebAppDiagnostics();
                loggingBuilder.AddApplicationInsights("ikey");
            });

            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.WithOrigins("https://prototypevideosyncingsite111.azurewebsites.net/",
                                                          "https://syncd.me/");
                                  });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRoomManagerSingleton, RoomManagerSingleton>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddControllersWithViews();
            services.AddApplicationInsightsTelemetry();
            services.Configure<AzureFileLoggerOptions>(Configuration.GetSection("AzureLogging"));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            /*var logger = loggerFactory.CreateLogger("Startup");

            logger.LogWarning("[VSY] Logger configured!");
            CTrace.logger = logger;*/
            

            app.UseSession();
            app.UseMiddleware<CustomMiddleware>();

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(30)
            };

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseWebSockets(webSocketOptions);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthorization();
            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
            });
        }
    }
}
