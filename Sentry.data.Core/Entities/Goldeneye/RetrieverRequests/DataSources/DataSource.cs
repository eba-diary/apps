using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    /// <summary>
    /// Base class for any type of retriever data source
    /// This class is mapped into the database utilizing the table per hierarchy strategy with SourceType as the discriminator value.
    /// http://nhibernate.info/doc/nhibernate-reference/inheritance.html
    /// </summary
    /// 

    // TODO: Remove Interface
    public abstract class DataSource : IDataSource, ISecurable
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        /// <summary>
        /// Discriminator value for nhibernate mapping
        /// </summary>
        public virtual string SourceType { get; set; }
        public virtual List<AuthenticationType> ValidAuthTypes { get; set; }
        public virtual AuthenticationType SourceAuthType { get; set; }

        public virtual Boolean IsUriEditable { get; set; }
        public virtual Uri BaseUri { get; set; }
        public virtual string KeyCode { get; set; }
        public virtual bool IsUserPassRequired { get; set; }

        public virtual bool IsSourceCompressed { get; set; }
        public virtual CompressionTypes CompressionType { get; set; }

        public virtual string HostFingerPrintKey { get; set; }
        public virtual int PortNumber { get; set; }

        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
        public virtual string Bucket { get; set; }

        #region Security

        public virtual bool IsSecured { get; set; }
        public virtual Security Security { get; set; }
        public virtual string PrimaryContactId { get; set; }
        public virtual bool AdminDataPermissionsAreExplicit { get; }  /* Will always be false for this entity type */
        public virtual ISecurable Parent { get; }

        #endregion


        public abstract Uri CalcRelativeUri(RetrieverJob Job);
        public abstract string GetDropPrefix(RetrieverJob Job);

        //https://stackoverflow.com/questions/2664245/identifying-nhibernate-proxy-classes
        //http://sessionfactory.blogspot.com/2010/08/hacking-lazy-loaded-inheritance.html
        public virtual Boolean Is<T>() where T : DataSource
        {
            return (this is T) ? true : false;
        }
        public abstract void Validate(RetrieverJob job, Sentry.Core.ValidationResults validationResults);

        public static class ValidationErrors
        {
            public const string googleApiRelativeUriIsBlank = "googleApiRelativeUriIsBlank";
            public const string httpsRequestMethodNotSelected = "httpsRequestMethodNotSelected";
            public const string httpsRequestDataFormatNotSelected = "httpsRequestDataFormateNotSelected";
            public const string httpsRequestBodyIsBlank = "httpsRequestBodyIsBlank";
            public const string httpsTargetFileNameIsBlank = "httpsTargetFileNameIsBlank";
            public const string httpsTargetFileNameContainsSpace = "httpsTargetFileNameContainsSpace";
            public const string ftpPatternNotSelected = "ftpPatternNotSelected";
            public const string relativeUriNotSpecified = "relativeUriNotSpecified";
            public const string relativeUriStartsWithForwardSlash = "relativeUriStartsWithForwardSlash";
        } 
    }
}
