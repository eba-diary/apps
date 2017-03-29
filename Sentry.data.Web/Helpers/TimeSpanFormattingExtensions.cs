﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Sentry.data.Web
{
    public static class TimeSpanFormattingExtensions
    {
        public static string ToReadableString(this TimeSpan span)
        {
            return string.Join(", ", span.GetReadableStringElements().Where((str) => ! string.IsNullOrWhiteSpace(str)));
        }
        
        private static IEnumerable<string> GetReadableStringElements(this TimeSpan span)
        {
            yield return GetDaysString((int)Math.Floor(span.TotalDays));
            yield return GetHoursString(span.Hours);
            yield return GetMinutesString(span.Minutes);
            yield return GetSecondsString(span.Seconds);
        }

        private static string GetDaysString(this int days)
        {
            if (days == 0)
            {
                return string.Empty;
            }

            if (days == 1)
            {
                return "1 day";
            }

            return string.Format("{0:0} days", days);
        }

        private static string GetHoursString(this int hours)
        {
            if (hours == 0)
            {
                return string.Empty;
            }

            if (hours == 1)
            {
                return "1 hour";
            }

            return string.Format("{0:0} hours", hours);
        }

        private static string GetMinutesString(this int minutes)
        {
            if (minutes == 0)
            {
                return string.Empty;
            }

            if (minutes == 1)
            {
                return "1 minute";
            }

            return string.Format("{0:0} minutes", minutes);
        }

        private static string GetSecondsString(this int seconds)
        {
            if (seconds == 0)
            {
                return string.Empty;
            }

            if (seconds == 1)
            {
                return "1 second";
            }

            return string.Format("{0:0} seconds", seconds);
        }
    }
}
