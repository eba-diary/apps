using Sentry.Core;
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
                return GlobalConstants.DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE;
            }
        }

        public override void Validate(RetrieverJob job, ValidationResults validationResults)
        {
            base.Validate(job, validationResults);

            if (job.JobOptions.HttpOptions.RequestMethod == HttpMethods.none)
            {
                validationResults.Add(ValidationErrors.httpsRequestMethodNotSelected, "Request method is required");
            }
        }
    }
}
