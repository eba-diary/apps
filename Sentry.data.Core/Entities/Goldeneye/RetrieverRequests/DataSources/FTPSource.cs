using Sentry.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sentry.data.Core
{
    public class FtpSource : DataSource
    {
        private Uri _baseUri;

        public FtpSource()
        {
            IsUriEditable = true;
            IsSourceCompressed = false;
            KeyCode = Guid.NewGuid().ToString().Substring(0, 13);

            //Control auth types which can be chosen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidAuthTypes = new List<AuthenticationType>();
            ValidAuthTypes.Add(new BasicAuthentication());
            ValidAuthTypes.Add(new AnonymousAuthentication());

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
            PortNumber = 21;

        }
        
        public override List<AuthenticationType> ValidAuthTypes { get; set; }

        //Setting Discriminator Value for NHibernate
        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSourceDiscriminator.FTP_SOURCE;
            }
        }
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
            if (String.IsNullOrWhiteSpace(job.RelativeUri))
            {
                validationResults.Add(ValidationErrors.relativeUriNotSpecified, "Relative Uri is required for FTP data sources");
            }
            if (job.JobOptions.FtpPattern == FtpPattern.None)
            {
                validationResults.Add(ValidationErrors.ftpPatternNotSelected, "The FTP Pattern field is required");
            }
        }
    }
}
