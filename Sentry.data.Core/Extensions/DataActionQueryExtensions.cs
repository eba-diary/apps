using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using System.Linq;

namespace Sentry.data.Core
{
    public static class DataActionQueryExtensions
    {        


        public static T GetAction<T>(this IQueryable<T> query, IDataFeatures dataFeatures, bool isHumanResources) where T : BaseAction
        {
            Logger.Info("Method <GetAction> Started");

            T result;

            if (isHumanResources)
            {
                result = query.GetHrAction();
            }
            else if (query is IQueryable<ProducerS3DropAction>)
            {
                result = dataFeatures.CLA3240_UseDropLocationV2.GetValue() ? query.GetDlstAction() : query.GetDataAction();
            }
            else
            {
                result = dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue() ? query.GetDlstAction() : query.GetDataAction();
            }

            Logger.Info("Method <GetAction> Ended");
            return result;
        }

        /// <summary>
        /// Returns the ProducerS3DropAction based on evaluationg of input values
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dataFeatures"></param>
        /// <param name="isHumanResources"></param>
        /// <returns></returns>
        //public static T GetActionObject<T>(this IQueryable<T> query, IDataFeatures dataFeatures, bool isHumanResources = false) where T : BaseAction
        //{
        //    return GetAction(dataFeatures, isHumanResources, query.GetHrAction(), query.GetDlstDropLocation(), query.GetDataDropLocation());
        //    //return isHumanResources
        //    //    ? query.GetHrDataDropLocation()
        //    //    : dataFeatures.CLA3240_UseDropLocationV2.GetValue() ? query.GetDlstDropLocation() : query.GetDataDropLocation();
        //}

        public static RawStorageAction GetRawStorageAction(this IQueryable<RawStorageAction> query, IDataFeatures dataFeatures, bool isHumanResources = false)
        {
            return isHumanResources
                ? query.GetHrRawStorage()
                : dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue() ? query.GetDlstRawStorage() : query.GetDataRawStorage();
        }

        public static QueryStorageAction GetQueryStorageAction(this IQueryable<QueryStorageAction> query, IDataFeatures dataFeatures, bool IsHumanResources = false)
        {
            return IsHumanResources
                ? query.GetHrQueryStorageAction()
                : dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue() ? query.GetDlstQueryStorage() : query.GetDataQueryStorage();
        }




        private static T GetHrAction<T>(this IQueryable<T> query) where T: BaseAction
        {
            Logger.Info("Method <GetHrAction> Started");

            int actionId;

            if (query is IQueryable<ProducerS3DropAction>)
            {
                actionId = 20;
            }
            else if (query is IQueryable<RawStorageAction>)
            {
                actionId = 16;
            }
            else if (query is IQueryable<QueryStorageAction>)
            {
                actionId = 17;
            }
            else if (query is IQueryable<ConvertToParquetAction>)
            {
                actionId = 19;
            }
            else if (query is IQueryable<XMLAction>)
            {
                actionId = 21;
            }
            else if (query is IQueryable<SchemaLoadAction>)
            {
                actionId = 18;
            }
            else
            {
                actionId = 0;
            }

            T result = query.FirstOrDefault(a => a.Id == actionId);


            //If HR Category and we do not find associated action, we throw an exception.  
            //  If this is a vaild action for HR, then GetHrAction would need to be adjusted
            //  to select appropriate action.
            if (result == null)
            {
                string actionType = query.First().GetType().Name;
                DataFlowStepNotImplementedException ex = new DataFlowStepNotImplementedException($"HR action not found for {actionType} action type");
                Logger.Warn($"DataFlowStep ({actionType}) not implmented for use wth HR category", ex);
                Logger.Info("Method <GetHrAction> Ended");
                throw ex;
            }

            
            return result;
        }

        private static T GetDataAction<T>(this IQueryable<T> query) where T: BaseAction
        {
            Logger.Info("Method <GetDataAction> Started");

            int actionId;

            if (query is IQueryable<ProducerS3DropAction>)
            {
                actionId = 12;
            }
            else if (query is IQueryable<RawStorageAction>)
            {
                actionId = 2;
            }
            else if (query is IQueryable<QueryStorageAction>)
            {
                actionId = 3;
            }
            else if (query is IQueryable<ConvertToParquetAction>)
            {
                actionId = 6;
            }
            else if (query is IQueryable<UncompressZipAction>)
            {
                actionId = 5;
            }
            else if (query is IQueryable<GoogleApiAction>)
            {
                actionId = 8;
            }
            else if (query is IQueryable<ClaimIQAction>)
            {
                actionId = 9;
            }
            else if (query is IQueryable<UncompressGzipAction>)
            {
                actionId = 10;
            }
            else if (query is IQueryable<FixedWidthAction>)
            {
                actionId = 11;
            }
            else if (query is IQueryable<XMLAction>)
            {
                actionId = 13;
            }
            else if (query is IQueryable<JsonFlatteningAction>)
            {
                actionId = 14;
            }
            else if (query is IQueryable<SchemaLoadAction>)
            {
                actionId = 4;
            }
            else if (query is IQueryable<SchemaMapAction>)
            {
                actionId = 7;
            }
            else if (query is IQueryable<S3DropAction>)
            {
                actionId = 1;
            }
            else
            {
                actionId = 0;
            }

            T result = query.FirstOrDefault(a => a.Id == actionId);

            if (result == null)
            {
                string actionType = query.First().GetType().Name;
                DataFlowStepNotImplementedException ex = new DataFlowStepNotImplementedException($"Action (type:{actionType}) not found for use with DATA bucket");
                Logger.Warn($"DataFlowStep (type:{actionType}) not implmented for use with DATA bucket", ex);
                Logger.Info("Method <GetHrAction> Ended");
                throw ex;
            }

            Logger.Info("Method <GetDataAction> Ended");
            return result;
        }

        private static T GetDlstAction<T>(this IQueryable<T> query) where T: BaseAction
        {
            Logger.Info("Method <GetDlstAction> Started");

            int actionId;

            if (query is IQueryable<ProducerS3DropAction>)
            {
                actionId = 15;
            }
            else if (query is IQueryable<RawStorageAction>)
            {
                actionId = 22;
            }
            else if (query is IQueryable<QueryStorageAction>)
            {
                actionId = 23;
            }
            else if (query is IQueryable<ConvertToParquetAction>)
            {
                actionId = 24;
            }
            else if (query is IQueryable<UncompressZipAction>)
            {
                actionId = 25;
            }
            else if (query is IQueryable<GoogleApiAction>)
            {
                actionId = 26;
            }
            else if (query is IQueryable<ClaimIQAction>)
            {
                actionId = 27;
            }
            else if (query is IQueryable<UncompressGzipAction>)
            {
                actionId = 28;
            }
            else if (query is IQueryable<FixedWidthAction>)
            {
                actionId = 29;
            }
            else if (query is IQueryable<XMLAction>)
            {
                actionId = 30;
            }
            else if (query is IQueryable<JsonFlatteningAction>)
            {
                actionId = 31;
            }
            else if (query is IQueryable<SchemaLoadAction>)
            {
                actionId = 32;
            }
            else
            {
                actionId = 0;
            }

            T result = query.FirstOrDefault(a => a.Id == actionId);

            if (result == null)
            {
                string actionType = query.First().GetType().Name;
                DataFlowStepNotImplementedException ex = new DataFlowStepNotImplementedException($"Action (type:{actionType}) not found for use with DLST bucket");
                Logger.Warn($"DataFlowStep (type:{actionType}) not implmented for use with DLST bucket", ex);
                Logger.Info("Method <GetHrAction> Ended");
                throw ex;
            }

            Logger.Info("Method <GetDlstAction> Ended");
            return result;
        }

        #region HR Methods
        /****************************************************************************************************
        HR RELATED ACTIONS
        *****************************************************************************************************/
        private static RawStorageAction GetHrRawStorage(this IQueryable<RawStorageAction> query)
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

        private static ProducerS3DropAction GetHrDataDropLocation(this IQueryable<ProducerS3DropAction> query)
        {

            return query.FirstOrDefault(a => a.Id == 20);
        }

        public static XMLAction GetHrXMLAction(this IQueryable<XMLAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 21);
        }
        #endregion


        #region Private Methods

        /// <summary>
        /// Returns the ProducerS3DropAction with an ID of 15, which is configured to use the sentry-dlst-*-droplocation-ae2 bucket
        /// </summary>
        private static ProducerS3DropAction GetDlstDropLocation(this IQueryable<ProducerS3DropAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 15);
        }

        /// <summary>
        /// Returns the ProducerS3DropAction with an ID of 12, which is configured to use the sentry-data-*-droplocation-ae2 bucket
        /// </summary>
        private static ProducerS3DropAction GetDataDropLocation(this IQueryable<ProducerS3DropAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 12);
        }

        /// <summary>
        /// Returns the RawStorageAction with an ID of 2, which is configured to use the sentry-data-*-dataset-ae2 bucket
        /// </summary>
        private static RawStorageAction GetDataRawStorage(this IQueryable<RawStorageAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 2);
        }

        // <summary>
        /// Returns the RawStorageAction with an ID of 22, which is configured to use the sentry-dlst-*-dataset-ae2 bucket
        /// </summary>
        public static RawStorageAction GetDlstRawStorage(this IQueryable<RawStorageAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 22);
        }

        /// <summary>
        /// Returns the QueryStorageAction with an ID of 3, which is configured to use the sentry-data-*-dataset-ae2 bucket
        /// </summary>
        private static QueryStorageAction GetDataQueryStorage(this IQueryable<QueryStorageAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 3);
        }

        // <summary>
        /// Returns the QueryStorageAction with an ID of 17, which is configured to use the sentry-dlst-*-dataset-ae2 bucket
        /// </summary>
        public static QueryStorageAction GetDlstQueryStorage(this IQueryable<QueryStorageAction> query)
        {
            return query.FirstOrDefault(a => a.Id == 17);
        }

        #endregion


    }
}
