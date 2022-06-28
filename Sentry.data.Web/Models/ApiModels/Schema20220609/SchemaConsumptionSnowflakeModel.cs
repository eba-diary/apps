﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sentry.data.Core;

namespace Sentry.data.Web.Models.ApiModels.Schema20220609
{
    public class SchemaConsumptionSnowflakeModel : SchemaConsumptionModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public override SchemaConsumptionTypeEnum SchemaConsumptionType => SchemaConsumptionTypeEnum.Snowflake;

        public string SnowflakeWarehouse { get; set; }
        public string SnowflakeStage { get; set; }
        public string SnowflakeDatabase { get; set; }
        public string SnowflakeSchema { get; set; }
        public string SnowflakeTable { get; set; }
        public string SnowflakeStatus { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SnowflakeConsumptionType SnowflakeType { get; set; }


        /// <summary>
        /// Implementation of the Visitor pattern.  Allows a class that implements <see cref="ISchemaConsumptionModelVisitor{T}"/>
        /// to act upon this class hierarchy.
        /// </summary>
        /// <typeparam name="T">The superclass type that the visitor will return</typeparam>
        /// <param name="v">The visitor class itself</param>
        /// <returns>A response from the visitor</returns>
        public override T Accept<T>(ISchemaConsumptionModelVisitor<T> v) => v.Visit(this);
    }
}