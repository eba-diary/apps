using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.SpamFactory
{
    class Watch
    {
        public static void Run(IDatasetContext _datasetContext)
        {
            //Get all events since last minute
            List<Event> events = _datasetContext.EventsSince(DateTime.Now.AddMinutes(-1));


            foreach (Event e in events)
            {
                e.IsProcessed = true;

            }

            _datasetContext.SaveChanges();

        }

        public static void Hourly(IDatasetContext _datasetContext)
        {
            //Get all events in the last hour
            List<Event> events = _datasetContext.EventsSince(DateTime.Now.AddHours(-1));
        }

        public static void Daily(IDatasetContext _datasetContext)
        {
            //Get all events in the last day
            List<Event> events = _datasetContext.EventsSince(DateTime.Now.AddDays(-1));


        }

        public static void Weekly(IDatasetContext _datasetContext)
        {
            //Get all events in the last week
            List<Event> events = _datasetContext.EventsSince(DateTime.Now.AddDays(-7));

        }



    }
}
