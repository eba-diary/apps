using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Helpers
{
    static class Utility
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
    }
}