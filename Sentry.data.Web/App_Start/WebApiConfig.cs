using Sentry.data.Web.Models.ApiModels.Schema20220609;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
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
            Sentry.WebAPI.Versioning.Register.ApiVersions(config, Sentry.data.Web.WebAPI.Version.v1); // Replace "1" with a different default API version if you desire
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

                    //Once we upgrade to .NET 6, these custom classes will no longer be necessary
                    //See https://stackoverflow.com/a/36200684/2768996
                    c.DocumentFilter(() => new PolymorphismDocumentFilter<SchemaConsumptionModel>(nameof(SchemaConsumptionModel.SchemaConsumptionType)));
                    c.SchemaFilter<PolymorphismSchemaFilter<SchemaConsumptionModel>>();

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

    public class PolymorphismSchemaFilter<T> : ISchemaFilter
    {
        private readonly Lazy<HashSet<Type>> derivedTypes = new Lazy<HashSet<Type>>(Init);

        private static HashSet<Type> Init()
        {
            var abstractType = typeof(T);
            var dTypes = abstractType.Assembly
                                     .GetTypes()
                                     .Where(x => x.IsSubclassOf(abstractType));

            var result = new HashSet<Type>();

            foreach (var item in dTypes)
                result.Add(item);

            return result;
        }

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (!derivedTypes.Value.Contains(type)) return;

            var clonedSchema = new Schema
            {
                properties = schema.properties,
                type = schema.type,
                required = schema.required
            };

            //remove the properties from the derived type that are inherited - some clients don't like them defined in both places
            foreach (var prop in typeof(T).GetProperties())
            {
                clonedSchema.properties.Remove(prop.Name);
            }

            //schemaRegistry.Definitions[typeof(T).Name]; does not work correctly in SwashBuckle
            var parentSchema = new Schema { @ref = "#/definitions/" + typeof(T).Name };

            schema.allOf = new List<Schema> { parentSchema, clonedSchema };

            //reset properties for they are included in allOf, should be null but code does not handle it
            schema.properties = new Dictionary<string, Schema>();
        }
    }

    public class PolymorphismDocumentFilter<T> : IDocumentFilter
    {
        private string discriminatorName;
        public PolymorphismDocumentFilter(string discriminatorName) {
            this.discriminatorName = discriminatorName;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, System.Web.Http.Description.IApiExplorer apiExplorer)
        {
            RegisterSubClasses(schemaRegistry, typeof(T));
        }

        private void RegisterSubClasses(SchemaRegistry schemaRegistry, Type abstractType)
        {
            Schema parentSchema;
            if(schemaRegistry.Definitions.TryGetValue(abstractType.FriendlyId(), out parentSchema))
            {
                //set up a discriminator property (it must be required)
                parentSchema.discriminator = discriminatorName;
                parentSchema.required = new List<string> { discriminatorName };

                if (!parentSchema.properties.ContainsKey(discriminatorName))
                    parentSchema.properties.Add(discriminatorName, new Schema { type = "string" });

                //register all subclasses
                var derivedTypes = abstractType.Assembly
                                               .GetTypes()
                                               .Where(x => x.IsSubclassOf(abstractType));

                foreach (var item in derivedTypes)
                    schemaRegistry.GetOrRegister(item);
            }
        }
    }
} 