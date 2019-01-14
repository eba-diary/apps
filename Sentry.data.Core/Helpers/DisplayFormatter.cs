using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Associates;

namespace Sentry.data.Core.Helpers
{
    /// <summary>
    /// Provides common area for any text formatting (i.e. phone numbers, dates, associate name)
    /// </summary>
    /// 
    public static class DisplayFormatter
    {
        public static string FormatAssociateName(Associate sentryAssociate)
        {
            // determine whether to use the associate's first or familiar name
            var assocName = (string.IsNullOrWhiteSpace(sentryAssociate.FamiliarName)) ? sentryAssociate.FirstName : sentryAssociate.FamiliarName;

            // tack on a space and the associate's last name
            assocName += " " + sentryAssociate.LastName;

            return assocName;
        }
    }
}