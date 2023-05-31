using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// Represents how a schema can be consumed from Snowflake
    /// </summary>
    public class SchemaConsumptionSnowflake : SchemaConsumption
    {
        public virtual string SnowflakeWarehouse { get; set; }
        public virtual string SnowflakeStage { get; set; }
        public virtual string SnowflakeDatabase { get; set; }
        public virtual string SnowflakeSchema { get; set; }
        public virtual string SnowflakeTable { get; set; }
        public virtual string SnowflakeStatus { get; set; }
        public virtual SnowflakeConsumptionType SnowflakeType { get; set; }
        public virtual DateTime LastChanged { get; set; }

        /// <summary>
        /// Implementation of the Visitor pattern.  Allows a class that implements <see cref="ISchemaConsumptionVisitor{T}"/>
        /// to act upon this class hierarchy.
        /// </summary>
        /// <typeparam name="T">The superclass type that the visitor will return</typeparam>
        /// <param name="v">The visitor class itself</param>
        /// <returns>A response from the visitor</returns>
        public override T Accept<T>(ISchemaConsumptionVisitor<T> v) => v.Visit(this);
    }
}
