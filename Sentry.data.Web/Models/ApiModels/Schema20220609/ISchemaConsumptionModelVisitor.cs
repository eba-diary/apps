using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Web.Models.ApiModels.Schema20220609
{
    /// <summary>
    /// Visitor pattern interface for <see cref="SchemaConsumptionModel"/> entities.
    /// This interface will need to have a separate method defined for each subclass of <see cref="SchemaConsumptionModel"/>
    /// </summary>
    /// <typeparam name="T">The superclass of the output of each Visit method</typeparam>
    public interface ISchemaConsumptionModelVisitor<out T>
    {
        /// <summary>
        /// Visit and act on a <see cref="SchemaConsumptionSnowflakeModel"/> class
        /// </summary>
        /// <param name="consumptionDetails">The instance of the <see cref="SchemaConsumptionSnowflakeModel"/> class</param>
        /// <returns>T or a sub-class of T that corresponds to <see cref="SchemaConsumptionSnowflakeModel"/></returns>
        T Visit(SchemaConsumptionSnowflakeModel consumptionDetails);
    }
}
