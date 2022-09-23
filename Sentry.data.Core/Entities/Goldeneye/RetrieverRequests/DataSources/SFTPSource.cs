using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using StructureMap;
using Sentry.Core;

namespace Sentry.data.Core
{
    public class SFtpSource : DataSource , IValidatable
    {
        private Uri _baseUri;

        public SFtpSource()
        {
            IsUriEditable = true;
            IsSourceCompressed = false;
            IsUserPassRequired = true;
            KeyCode = Guid.NewGuid().ToString().Substring(0, 13);

            //Control auth types which can be chosen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidAuthTypes = new List<AuthenticationType>();
            ValidAuthTypes.Add(new BasicAuthentication());

            //Control compression types which can be chossen for this source type.  As new
            // types are integrated, add the type to the list.
            ValidCompressionTypes = new List<CompressionTypes>();
            ValidCompressionTypes.Add(CompressionTypes.ZIP);
            ValidCompressionTypes.Add(CompressionTypes.GZIP);

            //Default created and modified to same datetime value
            DateTime curDTM = DateTime.Now;
            Created = curDTM;
            Modified = curDTM;

            //Default SFTP port number is 22
            PortNumber = 22;
        }

        public override List<AuthenticationType> ValidAuthTypes { get; set; }
        public override string SourceType
        {
            get
            {
                return GlobalConstants.DataSourceDiscriminator.SFTP_SOURCE;
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

        public virtual ValidationResults ValidateForDelete()
        {
            return new ValidationResults();
        }

        public virtual ValidationResults ValidateForSave()
        {
            ValidationResults vr = new ValidationResults();

            if (PortNumber == 0)
            {
                vr.Add(ValidationErrors.portNumberValueNonZeroValue, "FTP Data Sources requires Port Number greater than 0.");
            }

            return vr;
        }

        public class ValidationErrors
        {
            public const string portNumberValueNonZeroValue = "portNumberValueNonZeroValue";
            public const string distinctSourceName = "distinctSourceName";
            public const string validUriSyntax = "validUriSyntax";
        }
    }
}
