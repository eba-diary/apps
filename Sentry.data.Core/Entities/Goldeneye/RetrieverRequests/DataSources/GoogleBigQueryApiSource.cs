using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class GoogleBigQueryApiSource : HTTPSSource
    {
        public GoogleBigQueryApiSource()
        {
            IsUriEditable = true;
            IsSourceCompressed = false;
            KeyCode = Guid.NewGuid().ToString().Substring(0, 13);
            PagingEnabled = true;

            //Control auth types which can be chosen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidAuthTypes = new List<AuthenticationType>();
            ValidAuthTypes.Add(new OAuthAuthentication());

            ValidHttpMethods = new List<HttpMethods>();
            ValidHttpMethods.Add(HttpMethods.get);

            ValidHttpDataFormats = new List<HttpDataFormat>();
            ValidHttpDataFormats.Add(HttpDataFormat.json);

            //Control compression types which can be chossen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidCompressionTypes = new List<CompressionTypes>();

            //Default created and modified to same datetime value
            DateTime curDTM = DateTime.Now;
            Created = curDTM;
            Modified = curDTM;

            //Default Ftp Port is 21
            PortNumber = 443;
        }

        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSoureDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE;
            }
        }
    }
}
