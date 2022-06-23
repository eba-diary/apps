using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// Represents how a Schema can be consumed
    /// </summary>
    public abstract class SchemaConsumption
    {
        public virtual int SchemaConsumptionId { get; set; }

        /// <summary>
        /// Method to accept a visitor to this class hierarchy
        /// </summary>
        /// <typeparam name="T">The superclass of the output of each Visit method</typeparam>
        /// <param name="v">The visitor class itself</param>
        /// <returns>An object of <see cref="T"/></returns>
        public abstract T Accept<T>(ISchemaConsumptionVisitor<T> v);
    }
}
