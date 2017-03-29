using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using System.Data.SqlClient;
using NHibernate;
using Sentry.NHibernate;

namespace Sentry.data.Infrastructure
{
    public class AssetDynamicDetailsProvider : NHibernate.NHReadableStatelessDomainContext, IAssetDynamicDetailsProvider
    {
        public AssetDynamicDetailsProvider(IStatelessSession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<AssetDynamicDetailsProvider>();
        }

        public AssetDynamicDetails GetByAssetId(int assetId)
        {
            string sqlConnString = null;
            string sqlQueryString = null;
            switch (assetId)
            {
                case 1:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("CL_ODS_ConnectionString");
                    sqlQueryString = "SELECT TOP 1 Policy_ODS_ID FROM dbo.vw_POLICY";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                case 2:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("PL_ODS_ConnectionString");
                    sqlQueryString = "SELECT TOP 1 Policy_ODS_ID FROM dbo.vw_POLICY";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                case 3:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("Claim_ODS_ConnectionString");
                    sqlQueryString = "SELECT TOP 1 Claim_ODS_ID FROM dbo.vw_CLAIM";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                case 4:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("SERA_CL_ConnectionString");
                    sqlQueryString = "SELECT TOP 1 BatchAudit_DIM_ID FROM dbo.vw_BATCH_AUDIT_D";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                case 5:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("SERA_PL_ConnectionString");
                    sqlQueryString = "SELECT TOP 1 BatchAudit_DIM_ID FROM dbo.vw_BATCH_AUDIT_D";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                case 6:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("SERA_ENT_ConnectionString");
                    sqlQueryString = "SELECT TOP 1 BatchAudit_DIM_ID FROM dbo.vw_BATCH_AUDIT_D";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                case 7:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("PCR_CL_ConnectionString");
                    sqlQueryString = "select max(BatchProcess_DTE) as Sparta_LastRun_DTE from dbo.BATCH_HISTORY where SubSrcSys_ID IN (1,2,3,4)";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                case 8:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("PCR_PL_ConnectionString");
                    sqlQueryString = "SELECT TOP 1 PremiumTransactionBatch_ID from dbo.vw_PREMIUM_TRANSACTION";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                case 9:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("LASER_ConnectionString");
                    sqlQueryString = "SELECT TOP 1 BatchAudit_DIM_ID FROM dbo.vw_BATCH_AUDIT_D";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                case 10:
                    sqlConnString = Sentry.Configuration.Config.GetHostSetting("TDM_ConnectionString");
                    sqlQueryString = "SELECT TOP 1 ID FROM dbo.TDMLog";
                    return ExecuteQuery(sqlConnString, sqlQueryString);
                default:
                    throw new Exception("Unknown asset id " + assetId);
            }
        }

        private AssetDynamicDetails ExecuteQuery(string sqlConnStr, string sqlQueryStr)
        {
            SqlConnection sqlConn = null;
            try
            {
                sqlConn = new SqlConnection(sqlConnStr);
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand(sqlQueryStr, sqlConn);
                var sqlResult = sqlCmd.ExecuteScalar();

                // if we get here, the above query did not fail, and the Asset is serving data
                // TODO: try to get the lasst refresh date rather than dummy data here....
                return new AssetDynamicDetails(AssetState.Up, DateTime.Today.AddHours(1));

            } catch (Exception ex)
            {
                // TODO: log the exception... for now it's being eaten...
                Sentry.Common.Logging.Logger.Error("Unable to execute query (" + sqlQueryStr + ") against (" + sqlConnStr + ")", ex);

                if (sqlConn != null)
                {   // close connection
                    sqlConn.Close();
                    sqlConn.Dispose();
                    // we could connect, but could not execute the queryu
                }
                // TODO: try to get the lasst refresh date rather than dummy data here....
                return new AssetDynamicDetails(AssetState.Down, DateTime.Today.AddHours(-23));
            }
        }
    }
}
