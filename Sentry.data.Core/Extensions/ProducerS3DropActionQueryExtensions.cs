using Sentry.data.Core.Entities.DataProcessing;
using System.Linq;

namespace Sentry.data.Core
{
    public static class ProducerS3DropActionQueryExtensions
    {
        /// <summary>
        /// Returns the ProducerS3DropAction with an ID of 12, which is configured to use the sentry-data-*-droplocation-ae2 bucket
        /// </summary>
        public static ProducerS3DropAction GetDataDropLocation(this IQueryable<ProducerS3DropAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 12);
        }

        /// <summary>
        /// Returns the ProducerS3DropAction with an ID of 15, which is configured to use the sentry-dlst-*-droplocation-ae2 bucket
        /// </summary>
        public static ProducerS3DropAction GetDlstDropLocation(this IQueryable<ProducerS3DropAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 15);
        }


    }
}
