
using System;

namespace Sentry.data.Core.Helpers
{
    public static class DatabaseHelper
    {
        public static int SafeDatabaseInt(object row)
        {
            if (row != null && int.TryParse(row.ToString(), out int result))
            {
                return result;
            }

            return 0;
        }

        public static string SafeDatabaseString(object row)
        {
            if (row != null)
            {
                return row.ToString();
            }

            return string.Empty;
        }

        public static DateTime SafeDatabaseDate(object row)
        {
            if (row != null && DateTime.TryParse(row.ToString(), out DateTime result))
            {
                return result;
            }

            return DateTime.MinValue;
        }
    }
}