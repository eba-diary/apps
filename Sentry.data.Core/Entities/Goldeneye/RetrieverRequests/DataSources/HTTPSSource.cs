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
        private Uri _baseUri;
        private string _requestHeaders;

        public HTTPSSource()
        {
            IsUriEditable = true;
            IsSourceCompressed = false;
            KeyCode = Guid.NewGuid().ToString().Substring(0, 13);

            //Control auth types which can be chosen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidAuthTypes = new List<AuthenticationType>();
            ValidAuthTypes.Add(new AnonymousAuthentication());
            ValidAuthTypes.Add(new TokenAuthentication());
            ValidAuthTypes.Add(new OAuthAuthentication());

            ValidHttpMethods = new List<HttpMethods>();
            ValidHttpMethods.Add(HttpMethods.get);

            ValidHttpDataFormats = new List<HttpDataFormat>();
            ValidHttpDataFormats.Add(HttpDataFormat.json);

            //Control compression types which can be chossen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidCompressionTypes = new List<CompressionTypes>();
            ValidCompressionTypes.Add(CompressionTypes.ZIP);
            ValidCompressionTypes.Add(CompressionTypes.GZIP);

            //Default created and modified to same datetime value
            DateTime curDTM = DateTime.Now;
            Created = curDTM;
            Modified = curDTM;

            //Default Ftp Port is 21
            PortNumber = 443;
        }

        public override List<AuthenticationType> ValidAuthTypes { get; set; }
        //Setting Discriminator Value for NHibernate
        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSourceDiscriminator.HTTPS_SOURCE;
            }
        }
        public virtual List<HttpMethods> ValidHttpMethods { get; set; }
        public virtual List<HttpDataFormat> ValidHttpDataFormats { get; set; }
        public override AuthenticationType SourceAuthType { get; set; }
        public virtual List<CompressionTypes> ValidCompressionTypes { get; set; }
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
        // Only used for TokenAuthentication type
        public virtual string AuthenticationHeaderName { get; set; }
        // Only used for TokenAuthentication type
        public virtual string AuthenticationTokenValue { get; set; }
        public virtual string IVKey { get; set; }
        public virtual HttpMethods RequestMethod { get; set; }
        public virtual HttpDataFormat RequestDataFormat { get; set; }
        public virtual bool PagingEnabled { get; set; }

        #region OAuth
        public virtual string ClientId { get; set; }
        public virtual string ClientPrivateId { get; set; }
        public virtual string Scope { get; set; }
        public virtual string TokenUrl { get; set; }
        public virtual int TokenExp { get; set; }
        public virtual string CurrentToken { get; set; }
        public virtual DateTime ?CurrentTokenExp { get; set; }
        public virtual IList<OAuthClaim> Claims { get; set; }
        public virtual OAuthGrantType GrantType { get; set; }
        #endregion

        public virtual List<RequestHeader> RequestHeaders
        {
            get
            {
                if (String.IsNullOrEmpty(_requestHeaders))
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
    }
}
