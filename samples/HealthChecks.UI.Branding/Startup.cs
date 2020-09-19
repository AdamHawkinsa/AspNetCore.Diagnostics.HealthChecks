﻿using HealthChecks.UI.Client;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace HealthChecks.UI.Branding
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            //To add authentication and authorization using demo identityserver uncomment AddDemoAuthentication and RequireAuthorization lines

            services
                //.AddDemoAuthentication()
                .AddHealthChecks()
                .AddProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated: 100, tags: new[] { "process", "memory" })
                .AddCheck<RandomHealthCheck>("random1", tags: new[] { "random" })
                .AddCheck<RandomHealthCheck>("random2", tags: new[] { "random" })
                .Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(config =>
                {
                    config.Authority = "https://demo.identityserver.io";
                    config.Audience = "api";
                    config.SaveToken = true;

                }).Services
                .AddAuthorization(config =>
                {
                    config.DefaultPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    
                })
                .AddHealthChecksUI(setupSettings: setup =>
                {
                    setup.AddOidc(setup =>
                    {
                        setup.ClientId = "interactive.public.short";
                        setup.Authority = "https://demo.identityserver.io";
                        setup.Scope = "openid profile email api";
                    });
                    
                    setup.SetHeaderText("Branding Demo - Health Checks Status");
                    setup.AddHealthCheckEndpoint("endpoint1", "/health-random");
                    setup.AddHealthCheckEndpoint("endpoint2", "health-process");                    
                    //Webhook endpoint with custom notification hours, and custom failure and description messages

                    setup.AddWebhookNotification("webhook1", uri: "https://healthchecks2.requestcatcher.com/",
                            payload: "{ message: \"Webhook report for [[LIVENESS]]: [[FAILURE]] - Description: [[DESCRIPTIONS]]\"}",
                                restorePayload: "{ message: \"[[LIVENESS]] is back to life\"}",
                                shouldNotifyFunc: report => DateTime.UtcNow.Hour >= 8 && DateTime.UtcNow.Hour <= 23,
                                customMessageFunc: (report) =>
                               {
                                   var failing = report.Entries.Where(e => e.Value.Status == UIHealthStatus.Unhealthy);
                                   return $"{failing.Count()} healthchecks are failing";
                               }, customDescriptionFunc: report =>
                               {
                                   var failing = report.Entries.Where(e => e.Value.Status == UIHealthStatus.Unhealthy);
                                   return $"{string.Join(" - ", failing.Select(f => f.Key))} healthchecks are failing";
                               });

                    //Webhook endpoint with default failure and description messages

                    setup.AddWebhookNotification("webhook1", uri: "https://healthchecks.requestcatcher.com/",
                                                 payload: "{ message: \"Webhook report for [[LIVENESS]]: [[FAILURE]] - Description: [[DESCRIPTIONS]]\"}",
                                                 restorePayload: "{ message: \"[[LIVENESS]] is back to life\"}");

                }).AddInMemoryStorage()
                  .Services
                .AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting()
               .UseAuthentication()
               .UseAuthorization()
               .UseEndpoints(config =>
               {
                   config.MapHealthChecks("/health-random", new HealthCheckOptions
                   {
                       Predicate = r => r.Tags.Contains("random"),
                       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                   });

                   config.MapHealthChecks("/health-process", new HealthCheckOptions
                   {
                       Predicate = r => r.Tags.Contains("process"),
                       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                   });

                   config.MapHealthChecksUI(setup =>
                   {
                       setup.AddCustomStylesheet("dotnet.css");
                   })
                   //.RequireAuthorization("AuthUserPolicy")
                   ;

                   config.MapDefaultControllerRoute();
               });
        }
    }

    public class RandomHealthCheck
    : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (DateTime.UtcNow.Minute % 2 == 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            return Task.FromResult(HealthCheckResult.Unhealthy(description: $"The healthcheck {context.Registration.Name} failed at minute {DateTime.UtcNow.Minute}"));
        }
    }
}