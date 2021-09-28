using Sentry.data.Core.Entities.DataProcessing;
using System.Linq;

namespace Sentry.data.Core
{
    public static class DataActionQueryExtensions
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

        
        /****************************************************************************************************
        GORDON
        HR RELATED ACTIONS
        *****************************************************************************************************/
        public static RawStorageAction GetHrRawStorage(this IQueryable<RawStorageAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 16);
        }

        public static QueryStorageAction GetHrQueryStorageAction(this IQueryable<QueryStorageAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 17);
        }

        public static SchemaLoadAction GetHrSchemaLoadAction(this IQueryable<SchemaLoadAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 18);
        }

        public static ConvertToParquetAction GetHrConvertToParquetAction(this IQueryable<ConvertToParquetAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 19);
        }

        public static ProducerS3DropAction GetHrDataDropLocation(this IQueryable<ProducerS3DropAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 20);
        }

        public static XMLAction GetHrXMLAction(this IQueryable<XMLAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 21);
        }





    }
}
