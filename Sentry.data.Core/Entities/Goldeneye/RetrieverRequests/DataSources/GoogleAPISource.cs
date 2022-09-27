using Sentry.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class GoogleApiSource : HTTPSSource
    {
        /* listing of endpoint availble for Google Anaytics Reporting
         * https://developers.google.com/analytics/devguides/reporting/core/v4/rest/ 
         */

        public GoogleApiSource()
        {
            IsUriEditable = true;
            IsSourceCompressed = false;
            KeyCode = Guid.NewGuid().ToString().Substring(0, 13);
            PagingEnabled = false;

            //Control auth types which can be chosen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidAuthTypes = new List<AuthenticationType>();
            ValidAuthTypes.Add(new OAuthAuthentication());

            ValidHttpMethods = new List<HttpMethods>();
            ValidHttpMethods.Add(HttpMethods.get);
            ValidHttpMethods.Add(HttpMethods.post);

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
                return GlobalConstants.DataSoureDiscriminator.GOOGLE_API_SOURCE;
            }
        }

        public override void Validate(RetrieverJob job, ValidationResults validationResults)
        {
            ValidateBase(job, validationResults);

            if (job.JobOptions.HttpOptions.RequestMethod == HttpMethods.none)
            {
                validationResults.Add(ValidationErrors.httpsRequestMethodNotSelected, "Request method is required");
            }

            if (job.JobOptions.HttpOptions.RequestMethod == HttpMethods.post && job.JobOptions.HttpOptions.RequestDataFormat == HttpDataFormat.none)
            {
                validationResults.Add(ValidationErrors.httpsRequestDataFormatNotSelected, "Request Body Format is required");
            }

            if (job.JobOptions.HttpOptions.RequestMethod == HttpMethods.post && string.IsNullOrWhiteSpace(job.JobOptions.HttpOptions.Body))
            {
                validationResults.Add(ValidationErrors.httpsRequestBodyIsBlank, "Request body is required");
            }
        }

        protected internal virtual void ValidateBase(RetrieverJob Job, ValidationResults validationResults)
        {
            base.Validate(Job, validationResults);
        }
    }
}
