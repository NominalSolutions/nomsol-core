using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nomsol.core.api.extensions;
using nomsol.core.api.models.attributes;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace nomsol.core.api
{
    public static class Register
    {
        public static IServiceCollection AddMinimalApiVersioning(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
        {
            string _applicationName = configuration.GetValue<string>("ApplicationSettings:ApplicationName") ?? throw new ArgumentException("Variable ApplicationName Missing");
            string _applicationVersion = configuration.GetValue<string>("ApplicationSettings:Version") ?? throw new ArgumentException("Variable Version Missing");

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1);
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Version"),
                    new MediaTypeApiVersionReader("ver"));
            }).AddApiExplorer(
               options =>
               {
                   options.GroupNameFormat = "'v'V";
                   options.SubstituteApiVersionInUrl = true;
               });

            services.AddSwaggerGen();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(serviceProvider =>
                new ConfigureSwaggerOptions(
                    serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>(),
                    environment,
                    _applicationName,
                    _applicationVersion));

            return services;
        }

        public static IApplicationBuilder AddCustomSwagger(this IApplicationBuilder app, IConfiguration configuration, List<string> versions)
        {
            string? _applicationName = configuration.GetValue<string>("ApplicationSettings:ApplicationName");

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var l in versions)
                {
                    c.SwaggerEndpoint($"/swagger/{l}/swagger.json", $"{_applicationName} {l}");
                }

                c.DocumentTitle = _applicationName;

                // Disabling options for model expansion and try out option
                c.DefaultModelsExpandDepth(-1);
                c.DefaultModelExpandDepth(-1);
                c.SupportedSubmitMethods();

                // For custom UI
                c.InjectStylesheet(@"/resources/css/custom-swagger.css");
                c.InjectJavascript(@"/resources/scripts/custom-script.js");
            });

            return app;
        }                
    }
}
