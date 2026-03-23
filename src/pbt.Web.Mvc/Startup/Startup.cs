using Abp.AspNetCore;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Castle.Logging.Log4Net;
using Abp.Runtime.Validation;
using Castle.Facilities.Logging;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using pbt.Application.Cache;
using pbt.ApplicationUtils;
using pbt.Authentication.JwtBearer;
using pbt.ChangeLogger;
using pbt.Configuration;
using pbt.Identity;
using pbt.Web.Middlewares;
using pbt.Web.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace pbt.Web.Startup
{
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfigurationRoot _appConfiguration;

        public Startup(IWebHostEnvironment env)
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEntityChangeLoggerAppService, EntityChangeLoggerAppService>();
            // MVC
            services.AddControllersWithViews(
                    options =>
                    {
                        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                        options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
                    }
                );

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            services.AddScoped<IWebResourceManager, WebResourceManager>();

            services.AddSignalR();

            // Configure Abp and Dependency Injection
            services.AddAbpWithoutCreatingServiceProvider<pbtWebMvcModule>(
                // Configure Log4Net logging
                options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig(
                        _hostingEnvironment.IsDevelopment()
                            ? "log4net.config"
                            : "log4net.Production.config"
                        )
                )
            );

            services.AddMemoryCache();
            services.AddSingleton<ConfigAppCacheService>();
            services.AddSingleton<CookieService>();
           // ConfigureMassTransit(services);
        }

        private void ConfigureMassTransit(IServiceCollection services)
        {
            var entityConnectType = Convert.ToInt32(_appConfiguration["EntityAuditLog:ConnectType"]);
            switch (entityConnectType)
            {
                case (int)EntityAuditLogConnectType.RabbitMQ:
                    {
                        var rabbitHostname = _appConfiguration["EntityAuditLog:Hostname"];
                        var rabbitUser = _appConfiguration["EntityAuditLog:UserName"];
                        var rabbitPass = _appConfiguration["EntityAuditLog:Password"];
                        var queueName = _appConfiguration["EntityAuditLog:QueueName"];
                        services.AddMassTransit(mass =>
                        {
                            mass.UsingRabbitMq((context, cfg) =>
                            {
                                cfg.Host(rabbitHostname, "/", h =>
                                {
                                    h.Username(rabbitUser);
                                    h.Password(rabbitPass);

                                });
                                cfg.ConfigureEndpoints(context);
                            });
                        });

                    }
                    break;

                case (int)EntityAuditLogConnectType.HttpApi:
                    {
                        var apiUrl = _appConfiguration["EntityAuditLog:ApiUrl"];
                        var securityToken = _appConfiguration["EntityAuditLog:SecurityToken"];
                        services.AddHttpClient("EntityAuditLogHttpClient", client =>
                        {
                            client.BaseAddress = new Uri(apiUrl);
                            client.DefaultRequestHeaders.Add("Authorization", $"{securityToken}");
                        });
                    }
                    break;
                default:
                    break;
            }

        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAbp(); // Initializes ABP framework.

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            // Middleware xử lý Validation Exception
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (AbpValidationException ex)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";

                    var errors = ex.ValidationErrors?
                        .Select<System.ComponentModel.DataAnnotations.ValidationResult, object>(e => new
                        {
                            PropertyName = e.MemberNames.First(),
                            e.ErrorMessage
                        })
                        .ToList() ?? new List<object>();

                    var errorResponse = new
                    {
                        success = false,
                        error = new
                        {
                            message = "Validation failed!",
                            details = errors
                        }
                    };
                    await context.Response.WriteAsJsonAsync(errorResponse);
                }
            });

            app.UseRouting();

            app.UseAuthentication();

            //app.UseMiddleware<UserActivityMiddleware>();

            app.UseJwtTokenMiddleware();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AbpCommonHub>("/signalr");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
