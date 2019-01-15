using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class EventService : IEventService
    {
        /// <summary>
        /// Logs events to the Event table
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task CreateEventAsync(Event e)
        {
            StructureMap.IContainer _container;
            using (_container = Bootstrapper.Container.GetNestedContainer())
            {
                var _datasetContext = _container.GetInstance<IDatasetContext>();

                try
                {
                    _datasetContext.Merge<Event>(e);
                    _datasetContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to save event", ex);
                }

            }
        }
    }
}
