using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.NHibernate;
using NHibernate;
using NHibernate.Linq;

namespace Sentry.data.Infrastructure
{
    public class ODCFileProvider : NHWritableDomainContext, IODCFileProvider
    {
        public ODCFileProvider(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<ODCFileProvider>();
        }

        public string GetXMLString(ComponentElement ce)
        {
            string[] link = ce.Link.Split('|');

            string server_nme = link[0];
            string database_nme = link[1];
            string Cube_NME = ce.Name;
            string Env = "PROD";

            string xml = @"<html xmlns:o=""urn:schemas-microsoft-com:office:office""
                                xmlns=""http://www.w3.org/TR/REC-html40"">

                            <head>
                            <meta http-equiv=Content-Type content = ""text/x-ms-odc; charset=utf-8"" >
                            <meta name=ProgId content=ODC.Cube>
                            <meta name=SourceType content=OLEDB>
                            <meta name=Catalog content='" + database_nme + @"'>
                            <meta name=Table content=""'" + Cube_NME + @"'"">
                            <title>" + database_nme + " - " + Env + " - " + server_nme + @"</title>
                            <xml id=docprops>
                                <o:DocumentProperties
                                    xmlns:o=""urn:schemas-microsoft-com:office:office""
                                    xmlns=""http://www.w3.org/TR/REC-html40"">
                                    <o:Name>" + database_nme + " - " + Env + " - " + server_nme + @"</o:Name>
                                </o:DocumentProperties>
                            </xml>
                            <xml id=msodc>
                                <odc:OfficeDataConnection 
                                    xmlns:odc=""urn:schemas-microsoft-com:office:odc""
                                    xmlns=""http://www.w3.org/TR/REC-html40"">
                                    <odc:Connection odc:Type=""OLEDB"">
                                        <odc:ConnectionString>Provider=MSOLAP.4; Integrated Security=SSPI; Persist Security Info=True; Data Source=" + server_nme + "; Initial Catalog=" + database_nme + @"</odc:ConnectionString>
                                        <odc:CommandType>Cube</odc:CommandType>                   
                                        <odc:CommandText>" + Cube_NME + @"</odc:CommandText>
                                    </odc:Connection>                             
                                </odc:OfficeDataConnection>
                            </xml>
                            <style>
                            <!--                              
                                .ODCDataSource
                                {
                                    behavior: url(dataconn.htc);
                                }
                            -->
                            </style>";

            return xml;
        }
    }
}
