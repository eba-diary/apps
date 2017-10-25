using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.NHibernate;
using NHibernate;
using System.Data.SqlClient;

namespace Sentry.data.Infrastructure
{
    public class MetadataRepositoryProvider : NHReadableStatelessDomainContext, IMetadataRepositoryProvider
    {
        public MetadataRepositoryProvider(IStatelessSession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<MetadataRepositoryProvider>();
        }

        //public IList<DataAssetHealth> GetAssetHealthByName(string assetName)
        //{
        //    ISQLQuery query = Session.CreateSQLQuery(
        //        "SELECT DataAsset_NME, MAX(AssetUpdt_DTM), null, null, SourceSystem_VAL,null,0" +
        //        "FROM dbo.DataAssetHealth {dah} " +
        //        $"Where Cube_NME = '' and DataAsset_NME = '{assetName}' " +
        //        "GROUP BY DataAsset_NME, SourceSystem_VAL " +
        //        "order by 2 desc");
        //    query.AddEntity("dah", typeof(DataAssetHealth));
        //    query.ca
        //    var result = query.List<DataAssetHealth>();
        //    return result;
        //}

        public List<DataAssetHealth> GetByAssetName(string assetName)
        {
            string sqlConnString = null;
            string sqlQueryString = null;

            sqlConnString = Sentry.Configuration.Config.GetHostSetting("MetadataRepository_ConnectionString");
            sqlQueryString = $"SELECT  DataAsset_NME, MAX(AssetUpdt_DTM), SourceSystem_VAL FROM dbo.DataAssetHealth Where Cube_NME = '' and DataAsset_NME = '{assetName}' GROUP BY DataAsset_NME, SourceSystem_VAL";
            return ExecuteQuery(sqlConnString, sqlQueryString);
            
        }

        private List<DataAssetHealth> ExecuteQuery(string sqlConnStr, string sqlQueryStr)
        {
            SqlConnection sqlConn = null;
            List<DataAssetHealth> healthList = new List<DataAssetHealth>();
            try
            {
                sqlConn = new SqlConnection(sqlConnStr);
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand(sqlQueryStr, sqlConn);
                SqlDataReader reader = sqlCmd.ExecuteReader();
                while (reader.Read())
                {
                    healthList.Add(new DataAssetHealth(reader[0].ToString(), 
                        Convert.ToDateTime(reader[1]), 
                        null, 
                        null, 
                        reader[2].ToString(), 
                        DateTime.MinValue, 
                        0)
                        );
                }
                
                // if we get here, the above query did not fail, and the Asset is serving data
                // TODO: try to get the lasst refresh date rather than dummy data here....
                //return new AssetDynamicDetails(AssetState.Up, DateTime.Today.AddHours(1));

                return healthList;

            }
            catch (Exception ex)
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
                return healthList;
            }
        }
    }
}
