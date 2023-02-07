using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using System.Linq;

namespace Sentry.data.Core
{
    public static class DataActionQueryExtensions
    {        

        /// <summary>
        /// Returns appropriate DataAction after evaluating various conditions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="dataFeatures">Current feature flags</param>
        /// <param name="isHumanResources">Is this for processing HR data</param>
        /// <param name="namedEnvironmentType">Named environment of associated dataset</param>
        /// <param name="checkNamedEnvironmentType">Should interrogate named environment type</param>
        /// <returns></returns>
        public static T GetAction<T>(this IQueryable<T> query, IDataFeatures dataFeatures, bool isHumanResources, NamedEnvironmentType namedEnvironmentType, bool checkNamedEnvironmentType) where T : BaseAction
        {
            Logger.Info("Method <GetAction> Started");

            
            T result;
            if (string.IsNullOrWhiteSpace(dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue())
                    && checkNamedEnvironmentType && namedEnvironmentType == NamedEnvironmentType.NonProd)
            {
                result = isHumanResources ? query.GetHrActionNp() : query.GetDlstActionNp();
            }
            else
            {
                result = isHumanResources ? query.GetHrAction() : query.GetDlstAction();
            }

            Logger.Info("Method <GetAction> Ended");
            return result;
        }

        #region Private Methods
        private static T GetHrActionNp<T>(this IQueryable<T> query) where T : BaseAction
        {
            Logger.Info("Method <GetHrAction> Started");

            int actionId;

            if (query is IQueryable<ProducerS3DropAction>)
            {
                actionId = 40;
            }
            else if (query is IQueryable<RawStorageAction>)
            {
                actionId = 36;
            }
            else if (query is IQueryable<QueryStorageAction>)
            {
                actionId = 37;
            }
            else if (query is IQueryable<ConvertToParquetAction>)
            {
                actionId = 39;
            }
            else if (query is IQueryable<XMLAction>)
            {
                actionId = 41;
            }
            else if (query is IQueryable<SchemaLoadAction>)
            {
                actionId = 38;
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
                DataFlowStepNotImplementedException ex = new DataFlowStepNotImplementedException($"HR action not found for {actionType} nonprod action type");
                Logger.Warn($"DataFlowStep ({actionType}) not implmented for use wth HR category", ex);
                Logger.Info("Method <GetHrAction> Ended");
                throw ex;
            }
            return result;
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

        private static T GetDlstActionNp<T>(this IQueryable<T> query) where T : BaseAction
        {
            Logger.Info("Method <GetDlstAction> Started");

            int actionId;

            if (query is IQueryable<ProducerS3DropAction>)
            {
                actionId = 35;
            }
            else if (query is IQueryable<RawStorageAction>)
            {
                actionId = 42;
            }
            else if (query is IQueryable<QueryStorageAction>)
            {
                actionId = 43;
            }
            else if (query is IQueryable<ConvertToParquetAction>)
            {
                actionId = 44;
            }
            else if (query is IQueryable<UncompressZipAction>)
            {
                actionId = 45;
            }
            else if (query is IQueryable<GoogleApiAction>)
            {
                actionId = 46;
            }
            else if (query is IQueryable<ClaimIQAction>)
            {
                actionId = 47;
            }
            else if (query is IQueryable<UncompressGzipAction>)
            {
                actionId = 48;
            }
            else if (query is IQueryable<FixedWidthAction>)
            {
                actionId = 49;
            }
            else if (query is IQueryable<XMLAction>)
            {
                actionId = 50;
            }
            else if (query is IQueryable<JsonFlatteningAction>)
            {
                actionId = 51;
            }
            else if (query is IQueryable<SchemaLoadAction>)
            {
                actionId = 52;
            }
            else if (query is IQueryable<GoogleBigQueryApiAction>)
            {
                actionId = 53;
            }
            else if (query is IQueryable<GoogleSearchConsoleApiAction>)
            {
                actionId = 54;
            }
            else
            {
                actionId = 0;
            }

            T result = query.FirstOrDefault(a => a.Id == actionId);

            if (result == null)
            {
                string actionType = query.First().GetType().Name;
                DataFlowStepNotImplementedException ex = new DataFlowStepNotImplementedException($"NonProd Action (type:{actionType}) not found for use with DLST bucket");
                Logger.Warn($"DataFlowStep (type:{actionType}) not implmented for use with DLST bucket", ex);
                Logger.Info("Method <GetHrAction> Ended");
                throw ex;
            }

            Logger.Info("Method <GetDlstAction> Ended");
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
            else if (query is IQueryable<GoogleBigQueryApiAction>)
            {
                actionId = 33;
            }
            else if (query is IQueryable<GoogleSearchConsoleApiAction>)
            {
                actionId = 34;
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

        #endregion
    }
}
