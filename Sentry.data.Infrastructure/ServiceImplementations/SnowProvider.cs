using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Sentry.data.Core;
using Sentry.Common.Logging;
using Sentry.data.Core.GlobalEnums;
using System.Diagnostics;
using Snowflake.Data.Client;
using Sentry.Configuration;
using System.Linq;

namespace Sentry.data.Infrastructure 
{
    public class SnowProvider : ISnowProvider
    {


        public void GetTopNRows()
        {

            int x = 1 + 1;

            try
            {

                using (var connection = new SnowflakeDbConnection())
                {

                    connection.ConnectionString = Config.GetHostSetting("SnowConnectionString");
                    connection.Password = GetSecureString(Config.GetHostSetting("SnowPassword"));

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT count(*) FROM DATA_QUAL.CLAIM.vw_FNOL_CLAUTO";
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string columnTest = reader.GetString(0);
                    }
                    if (reader != null)
                        reader.Close();
                    
                }
            }
            catch (AggregateException aggEx)
            {
                //aggEx.Handle(inner =>
                //{
                //    Logger.Error("Inner aggregate execution", inner);
                //    //return true;
                //});
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to execute snowflake reader", ex);
                throw;
            }




        }

        private System.Security.SecureString GetSecureString(string str)
        {
            var secureStr = new System.Security.SecureString();
            if (str.Length > 0)
                str.ToCharArray().ToList().ForEach(f => secureStr.AppendChar(f));
            return secureStr;
        }






    }
}
