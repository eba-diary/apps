using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using System.Data.Odbc;
using Sentry.Common.Logging;
using System.Diagnostics;
using Sentry.data.Core.Exceptions;

namespace Sentry.data.Infrastructure
{
    public class HiveOdbcProvider : IHiveOdbcProvider
    {
        public System.Data.DataTable GetTopNRows(FileSchemaDto schemaDto, int rows)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string hiveDatabase = schemaDto.HiveDatabase;
            string hiveTable = schemaDto.HiveTable;
            System.Data.DataTable results = new System.Data.DataTable();

            //Build Connection String

            StringBuilder connSb = new StringBuilder();
            connSb.Append($"Driver={{{Configuration.Config.GetHostSetting("HiveOdbcDriver")}}};");

            //Is Zookeeper 
            if (Configuration.Config.GetHostSetting("UseServiceDiscoveryMode") == "true")
            {
                connSb.Append("ServiceDiscoveryMode=1;");
                connSb.Append($"ZKNamespace={Configuration.Config.GetHostSetting("ZookeeperNamesspace")};");
            }

            connSb.Append($"Host={Configuration.Config.GetHostSetting("HiveServerHost")};");

            if (Configuration.Config.GetHostSetting("HiveOdbcUseKerberos") == "true")
            {
                connSb.Append("AuthMech=1;");
                connSb.Append("KrbHostFQDN=_HOST;");
                connSb.Append($"KrbServiceName={Configuration.Config.GetHostSetting("HiveOdbcKerberosRealm")};");
            }

            Logger.Debug(connSb.ToString());

            try
            {
                using (OdbcConnection connection = new OdbcConnection(connSb.ToString()))
                {
                    var connOpenStart = stopWatch.ElapsedMilliseconds;
                    connection.Open();
                    var connOpenEnd = stopWatch.ElapsedMilliseconds;

                    Logger.Debug($"open connection time : {connOpenEnd - connOpenStart}(ms)");

                    var adp = new OdbcDataAdapter($"SELECT * FROM {hiveDatabase}.{hiveTable} limit {rows.ToString()}", connection);
                    var ds = new System.Data.DataSet();
                    adp.Fill(ds);

                    results = ds.Tables[0];
                }
            }
            catch(OdbcException oex)
            {
                if (oex.Message.Contains("Table or view not found"))
                {
                    Logger.Error("Table or view not found", oex);
                    throw new HiveTableViewNotFoundException("Table or view not found", oex);
                }
                else
                {
                    Logger.Error("Odbc query exception", oex);
                    throw new HiveQueryException("Hive ODBC query exception", oex);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("failed hiveodbc query", ex);
                throw;
            }

            Logger.Info($"query completed", new DoubleVariable("queryduration", stopWatch.Elapsed.TotalSeconds), new LongVariable("numberofrows", rows));
            return results;
        }
    }
}
