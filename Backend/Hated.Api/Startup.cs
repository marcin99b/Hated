﻿using System;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hated.Infrastructure.Extensions;
using Hated.Infrastructure.IoC;
using Hated.Infrastructure.Mongo;
using Hated.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Swashbuckle.AspNetCore.Swagger;

namespace Hated.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        private IContainer ApplicationContainer { get; set; }
        
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAny",
                    x =>
                    {
                        x.AllowAnyOrigin();
                        x.AllowAnyMethod();
                        x.AllowAnyHeader();
                        x.AllowCredentials();
                    });
            });

            var jwtSettings = Configuration.GetSettings<JwtSettings>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                        SaveSigninToken = true
                    };
                });
            services.AddAuthorization(x => x.AddPolicy("admin", policy => policy.RequireRole("admin")));
            services.AddMvc();
            services.AddSwaggerGen(x =>
                {
                    x.SwaggerDoc("v1", new Info
                    {
                        Title = "Hated.Api",
                        Version = "v1"
                    });
                    x.AddSecurityDefinition("Bearer", new ApiKeyScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey"
                    });
                }
            );

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule(new ContainerModule(Configuration));
            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifeTime, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();
            ConfigureSerilog();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            MongoConfigurator.Initialize();
            app.UseCors("AllowAny");
            app.UseAuthentication();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "Hated.Api"));
            appLifeTime.ApplicationStopped.Register(() => ApplicationContainer.Dispose());
        }

        private void AddSwaggerService(IServiceCollection services)
        {
            
        }

        private void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration().Enrich
                .FromLogContext()
                .WriteTo.File("Logs/logs.txt")
                .WriteTo.Elasticsearch()
                .CreateLogger();
        }
    }
}
