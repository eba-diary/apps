using Microsoft.Win32;
using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;

namespace TESTING_CONSOLE
{
    public class Class1
    {
        static void Main(string[] args)
        {
            //Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));

            Logger.Info("Starting TESTING_CONSOLE");
            //Call your bootstrapper to initialize your application
            //Bootstrapper.Init();

            List<int> intList = new List<int>() { 1, 2, 3 };

            Console.WriteLine("[{0}]", string.Join(", ", intList.Select(e => e.ToString()).ToArray()));

            //("[{0}]", string.Join(", ", yourArray))

            var conn = new OdbcConnection
            {
                //ConnectionString = @"Driver={Cloudera ODBC Driver for Apache Hive};  
                //                        ServiceDiscoveryMode=1;
                //                        Host=awe-q-apspml-01:2181,awe-q-apspml-02:2181,awe-q-apspml-03:2181;
                //                        ZKNamespace=hiveserver2;"
                //AuthenticationError

                //ConnectionString = @"Driver={Cloudera ODBC Driver for Apache Hive};  
                //                        ServiceDiscoveryMode=1;
                //                        Host=awe-q-apspml-01:2181,awe-q-apspml-02:2181,awe-q-apspml-03:2181;
                //                        ZKNamespace=hiveserver2;"

                //ConnectionString = @"Driver={Cloudera ODBC Driver for Apache Hive};
                //                        ServiceDiscoveryMode=1;
                //                        Host=awe-q-apspml-02.sentry.com:2181;
                //                        ZKNamespace=hive"

                //ConnectionString = @"Driver={Cloudera ODBC Driver for Apache Hive};
                //                        DSN=DSCQUAL_Sentry;"

                //ConnectionString = @"DSN=DSCQUAL_Sentry;"

                ConnectionString = @"Driver={Cloudera ODBC Driver for Apache Hive};
                                        ServiceDiscoveryMode=1;
                                        ZKNamespace=hiveserver2;
                                        Host=awe-q-apspml-02.sentry.com:2181;
                                        AuthMech=1;
                                        KrbHostFQDN=_HOST;
                                        KrbServiceName=hive;"
            };

            try
            {

                ///// <summary>
                ///// Gets the ODBC driver names from the registry.
                ///// </summary>
                ///// <returns>a string array containing the ODBC driver names, if the registry key is present; null, otherwise.</returns>
                string[] odbcDriverNames = null;
                using (RegistryKey localMachineHive = Registry.LocalMachine)
                using (RegistryKey odbcDriversKey = localMachineHive.OpenSubKey(@"SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers"))
                {
                    if (odbcDriversKey != null)
                    {
                        odbcDriverNames = odbcDriversKey.GetValueNames();
                    }
                }

                //foreach (string ODBCDriver in odbcDriverNames)
                //{
                //    Console.WriteLine(ODBCDriver);
                //}



                conn.Open();
                var adp = new OdbcDataAdapter("select * from dsc_sentry.dsctesting_csvtest limit 2",conn);
                var ds = new System.Data.DataSet();
                adp.Fill(ds);

                foreach (var table in ds.Tables)
                {
                    var dataTable = table as System.Data.DataTable;

                    if (dataTable == null)
                        continue;

                    var dataRows = dataTable.Rows;

                    if (dataRows == null)
                        continue;

                    //log.Info("Records found " + dataTable.Rows.Count);
                    Console.WriteLine("Records found " + dataTable.Rows.Count);
                    int rowCnt = 1;
                    int columns = dataTable.Columns.Count;

                    foreach (var row in dataRows)
                    {
                        var dataRow = row as System.Data.DataRow;

                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine($"*** Row {rowCnt++.ToString()} Data ***");

                        for (int i = 0; i < columns - 1; i++)
                        {
                            sb.AppendLine($"{dataTable.Columns[i].ColumnName}:{dataRow[i].ToString()}");
                        }

                        if (dataRow == null)
                            continue;

                        Console.WriteLine(sb.ToString());

                        //log.Info(dataRow[0].ToString() + " " + dataRow[1].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                conn.Close();
            }


            //Logger.Info("Console App completed successfully.");
        }
    }
}
