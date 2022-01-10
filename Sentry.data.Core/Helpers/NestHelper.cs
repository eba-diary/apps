using Nest;
using System;
using System.Linq;

namespace Sentry.data.Core
{
    public static class NestHelper
    {
        public static Nest.Fields GlobalSearchFields<T>()
        {
            return Infer.Fields(typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(GlobalSearchField))).ToArray());
        }
    }
}
