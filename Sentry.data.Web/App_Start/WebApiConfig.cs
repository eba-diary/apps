using Swashbuckle.Application;
using Swashbuckle.Swagger;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;

namespace Sentry.data.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var constraintResolver = new DefaultInlineConstraintResolver();
            Sentry.WebAPI.Versioning.Register.ConstraintMap(constraintResolver);
            Sentry.WebAPI.Versioning.Register.ApiVersions(config, Sentry.data.Web.WebAPI.Version.v2); // Replace "1" with a different default API version if you desire
            config.MapHttpAttributeRoutes(constraintResolver);

            // Web API exception handling/logging
            config.Services.Replace(typeof(IExceptionHandler), new UnhandledExceptionHandler());
            config.Services.Add(typeof(IExceptionLogger), new UnhandledExceptionLogger());


            config.EnableSwagger("{apiVersion}/swagger", c =>
                {
                    Sentry.WebAPI.Versioning.Register.SwaggerMultipleApiVersions(config, c, "Data.sentry.com");

                    c.BasicAuth("basic").Description("Basic HTTP Authentication");

                    c.IncludeXmlComments(GetXmlCommentsPath());

                    c.DescribeAllEnumsAsStrings(camelCase: false);

                    c.DocumentFilter<CustomDocumentFilter>();

                })
                .EnableSwaggerUi(c =>
                {
                    c.EnableDiscoveryUrlSelector();
                });

        }

        private static string GetXmlCommentsPath()
        {
            return $"{System.AppDomain.CurrentDomain.BaseDirectory}\\bin\\Sentry.data.Web.xml";
        }
    }

    public class CustomDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            var paths = swaggerDoc.paths.OrderBy(e => e.Key).ToList();
            swaggerDoc.paths = paths.ToDictionary(e => e.Key, e => e.Value);
        }
    }
} 