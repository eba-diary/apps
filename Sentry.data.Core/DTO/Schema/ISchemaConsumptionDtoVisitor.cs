using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// Visitor pattern interface for <see cref="SchemaConsumptionDto"/> entities.
    /// This interface will need to have a separate method defined for each subclass of <see cref="SchemaConsumptionDto"/>
    /// </summary>
    /// <typeparam name="T">The superclass of the output of each Visit method</typeparam>
    public interface ISchemaConsumptionDtoVisitor<out T>
    {
        /// <summary>
        /// Visit and act on a <see cref="SchemaConsumptionSnowflakeDto"/> class
        /// </summary>
        /// <param name="consumptionDetails">The instance of the <see cref="SchemaConsumptionSnowflakeDto"/> class</param>
        /// <returns>T or a sub-class of T that corresponds to <see cref="SchemaConsumptionSnowflakeDto"/></returns>
        T Visit(SchemaConsumptionSnowflakeDto consumptionDetails);
    }
}
