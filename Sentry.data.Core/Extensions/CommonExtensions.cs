﻿using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class CommonExtensions
    {

        public static IEnumerable<IList<T>> Split<T>(this IList<T> lst, int subListSize)
        {
            List<IList<T>> lists = new List<IList<T>>();
            int counter = 0;

            while (counter <= lst.Count)
            {
                lists.Add(lst.Skip(counter).Take(subListSize).ToList());

                counter += subListSize;
            }

            return lists;
        }

    }
}