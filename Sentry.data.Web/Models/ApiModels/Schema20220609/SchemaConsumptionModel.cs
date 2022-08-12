using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Schema20220609
{
    [JsonConverter(typeof(JsonSubtypes), nameof(SchemaConsumptionType))]
    [JsonSubtypes.KnownSubType(typeof(SchemaConsumptionSnowflakeModel), nameof(SchemaConsumptionTypeEnum.SchemaConsumptionSnowflakeModel))]
    public abstract class SchemaConsumptionModel
    {
        public int SchemaConsumptionId { get; set; }

        public virtual string SchemaConsumptionType { get; }

        /// <summary>
        /// Method to accept a visitor to this class hierarchy
        /// </summary>
        /// <typeparam name="T">The superclass of the output of each Visit method</typeparam>
        /// <param name="v">The visitor class itself</param>
        /// <returns>An object of <see cref="T"/></returns>
        public abstract T Accept<T>(ISchemaConsumptionModelVisitor<T> v);
    }
}