using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.SpamFactory
{
    class Watch
    {

        public static void Run()
        {






            Task.Factory.StartNew(() => Hourly());


        }

        public static void Hourly()
        {
            //Get all events in the last hour


        }

        public static void Daily()
        {
            //Get all events in the last day



        }

        public static void Weekly()
        {
            //Get all events in the last week



        }



    }
}
