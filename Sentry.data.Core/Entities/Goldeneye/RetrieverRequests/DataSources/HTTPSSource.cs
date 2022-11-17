using Newtonsoft.Json;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sentry.data.Core
{
    public class HTTPSSource : DataSource
    {
        #region Fields
        private Uri _baseUri;
        private string _requestHeaders;
        #endregion

        #region Constructor
        public HTTPSSource()
        {
            IsUriEditable = true;
            IsSourceCompressed = false;
            KeyCode = Guid.NewGuid().ToString().Substring(0, 13);

            //Control auth types which can be chosen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidAuthTypes = new List<AuthenticationType>
            {
                new AnonymousAuthentication(),
                new TokenAuthentication(),
                new OAuthAuthentication()
            };

            ValidHttpMethods = new List<HttpMethods>
            {
                HttpMethods.get
            };

            ValidHttpDataFormats = new List<HttpDataFormat>
            {
                HttpDataFormat.json
            };

            //Control compression types which can be chossen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidCompressionTypes = new List<CompressionTypes>
            {
                CompressionTypes.ZIP,
                CompressionTypes.GZIP
            };

            //Default created and modified to same datetime value
            DateTime curDTM = DateTime.Now;
            Created = curDTM;
            Modified = curDTM;

            //Default Ftp Port is 21
            PortNumber = 443;
        }
        #endregion

        #region DataSource Overrides
        public override string SourceType { get => GlobalConstants.DataSourceDiscriminator.HTTPS_SOURCE; }
        public override List<AuthenticationType> ValidAuthTypes { get; set; }
        public override AuthenticationType SourceAuthType { get; set; }
        public override Uri BaseUri
        {
            get
            {
                return _baseUri;
            }
            set
            {
                //https://github.com/nhibernate/nhibernate-core/blob/466ee0d29b19e1b77b734791e4bd061d58c52a6b/src/NHibernate/Type/UriType.cs
                Uri u = new Uri(value.ToString());
                _baseUri = new Uri(u.ToString());
            }
        }
        #endregion

        #region Properties
        public virtual List<HttpMethods> ValidHttpMethods { get; set; }
        public virtual List<HttpDataFormat> ValidHttpDataFormats { get; set; }
        public virtual List<CompressionTypes> ValidCompressionTypes { get; set; }
        // Only used for TokenAuthentication type
        public virtual string AuthenticationHeaderName { get; set; }
        // Only used for TokenAuthentication type
        public virtual string AuthenticationTokenValue { get; set; }
        public virtual string IVKey { get; set; }
        public virtual HttpMethods RequestMethod { get; set; }
        public virtual HttpDataFormat RequestDataFormat { get; set; }
        public virtual IList<DataSourceToken> Tokens { get; set; }
        public virtual string ClientId { get; set; }
        public virtual string ClientPrivateId { get; set; }
        public virtual OAuthGrantType GrantType { get; set; }
        public virtual List<RequestHeader> RequestHeaders
        {
            get
            {
                if (string.IsNullOrEmpty(_requestHeaders))
                {
                    return null;
                }
                else
                {
                    List<RequestHeader> a = JsonConvert.DeserializeObject<List<RequestHeader>>(_requestHeaders);
                    return a;
                }
            }
            set
            {
                _requestHeaders = JsonConvert.SerializeObject(value);
            }
        }
        #endregion

        #region Methods
        public override Uri CalcRelativeUri(RetrieverJob Job)
        {
            return new Uri(Path.Combine(BaseUri.ToString(), Job.RelativeUri).ToString());
        }

        public override string GetDropPrefix(RetrieverJob Job)
        {
            throw new NotImplementedException();
        }

        public override void Validate(RetrieverJob job, ValidationResults validationResults)
        {
            if (string.IsNullOrWhiteSpace(job.JobOptions.TargetFileName))
            {
                validationResults.Add(ValidationErrors.httpsTargetFileNameIsBlank, "Target file name is required for HTTPS data sources");
            }
            else if (job.JobOptions.TargetFileName.Contains(" "))
            {
                validationResults.Add(ValidationErrors.httpsTargetFileNameContainsSpace, "Target file name cannot contain spaces");
            }

            if (string.IsNullOrWhiteSpace(job.RelativeUri))
            {
                validationResults.Add(ValidationErrors.relativeUriNotSpecified, "Relative Uri is required for HTTPS data sources");
            }
            else if (job.RelativeUri.StartsWith("/"))
            {
                validationResults.Add(ValidationErrors.relativeUriStartsWithForwardSlash, "Relative Uri cannot start with '/' for HTTPS data sources");
            }
        }
        #endregion
    }
}
