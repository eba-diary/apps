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
using Microsoft.Win32;

namespace Sentry.data.Infrastructure
{
    public class HiveOdbcProvider : IHiveOdbcProvider
    {
        public OdbcConnection GetConnection(string database)
        {

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

            connSb.Append($"Initial Catalog={database};");

            Logger.Debug(connSb.ToString());

            //List out available drivers
            StringBuilder driverSb = new StringBuilder();
            using (RegistryKey localMachineHive = Registry.LocalMachine)
            using (RegistryKey odbcDriversKey = localMachineHive.OpenSubKey(@"SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers"))
            {
                if (odbcDriversKey != null)
                {
                    Array.ForEach(odbcDriversKey.GetValueNames(), s => driverSb.Append($"{s}; "));
                }
            }
            Logger.Debug(driverSb.ToString());

            return new OdbcConnection(connSb.ToString());
        }

        public bool CheckTableExists(OdbcConnection conn, string table)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var queryString = $"SHOW TABLES LIKE '*{table}*'";

            return CheckExists(conn, queryString, table);
        }
        public bool CheckViewExists(OdbcConnection conn, string view)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var queryString = $"SHOW VIEWS LIKE '*{view}*'";

            return CheckExists(conn, queryString, view);
        }

        public System.Data.DataTable GetTopNRows(OdbcConnection conn, string table, int rows)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            System.Data.DataTable results = new System.Data.DataTable();           

            try
                {
                using (OdbcConnection connection = conn)
                {
                    var connOpenStart = stopWatch.ElapsedMilliseconds;
                    connection.Open();
                    var connOpenEnd = stopWatch.ElapsedMilliseconds;

                    Logger.Debug($"open connection time : {connOpenEnd - connOpenStart}(ms)");

                    //var queryString = $"SELECT * FROM {database}.{table} limit {rows.ToString()}";
                    var queryString = $"SELECT * FROM {table} limit {rows.ToString()}";

                    Logger.Debug($"Hive query: {queryString}");

                    var queryStart = stopWatch.ElapsedMilliseconds;
                    var adp = new OdbcDataAdapter(queryString, connection);
                    var ds = new System.Data.DataSet();
                    adp.Fill(ds);
                    var queryEnd = stopWatch.ElapsedMilliseconds;

                    Logger.Debug($"query time : {queryEnd - queryStart}(ms)");

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

        private bool CheckExists(OdbcConnection conn, string queryString, string compareObject)
        {
            System.Data.DataTable results;

            try
            {
                using (OdbcConnection connection = conn)
                {
                    connection.Open();

                    var adp = new OdbcDataAdapter(queryString, connection);
                    var ds = new System.Data.DataSet();
                    adp.Fill(ds);

                    results = ds.Tables[0];

                    foreach (System.Data.DataRow dr in results.Rows)
                    {
                        foreach (System.Data.DataColumn col in results.Columns)
                        {
                            if (dr[col].ToString().ToLower() == compareObject.ToLower())
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
            catch (OdbcException oex)
            {
                Logger.Error("Odbc query exception", oex);
                throw new HiveQueryException("Hive ODBC query exception", oex);
            }
            catch (Exception ex)
            {
                Logger.Error("failed hiveodbc query", ex);
                throw;
            }
        }
    }
}
