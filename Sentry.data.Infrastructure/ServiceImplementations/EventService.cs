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
        public IDatasetContext _datasetContext;

        public EventService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        /// <summary>
        /// Logs events to the Event table
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>        
        private void CreateEvent(Event e)
        {
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public async Task CreateViewSchemaEditSuccessEvent(int configId, string userId, string reason)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);

            Event e = new Event();
            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.DataConfig = configId;
            e.Dataset = dfc.ParentDataset.DatasetId;
            e.UserWhoStartedEvent = userId;
            e.Reason = reason;

            CreateEvent(e);
        }
    }
}
