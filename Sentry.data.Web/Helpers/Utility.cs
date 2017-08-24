using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Helpers
{
    public static class Utility
    {
        public static List<T> IntersectAllIfEmpty<T>(params IEnumerable<T>[] lists)
        {
            IEnumerable<T> results = null;

            lists = lists.Where(l => l.Any()).ToArray();

            if (lists.Length > 0)
            {
                results = lists[0];

                for (int i = 1; i < lists.Length; i++)
                    results = results.Intersect(lists[i]);
            }
            else
            {
                results = new T[0];
            }

            List<T> var = results.ToList();

            //return results;
            return var;
        }

        public static string TimeDisplay(DateTime dt)
        {
            string result;
            TimeSpan span = DateTime.Now - dt;

            if (span.Days > 365)
            {
                int years = (span.Days / 365);
                result = string.Format("{0} {1} ago", years, years == 1 ? "year" : "years");
            }
            else if (span.Days > 30)
            {
                int months = (span.Days / 30);
                result = string.Format("{0} {1} ago", months, months == 1 ? "month" : "months");
            }
            else if (span.Days > 0)
            {
                result = string.Format("{0} {1} ago", span.Days, span.Days == 1 ? "day" : "days");
            }
            else if (span.Hours > 0)
            {
                result = string.Format("{0} {1} ago", span.Hours, span.Hours == 1 ? "hour" : "hours");
            }
            else if (span.Minutes > 0)
            {
                result = string.Format("{0} {1} ago", span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
            }
            else if (span.Seconds > 5)
            {
                result = string.Format("{0} seconds ago", span.Seconds);
            }
            else
            {
                result = "just now";
            }

            return result;
        }
    }

}