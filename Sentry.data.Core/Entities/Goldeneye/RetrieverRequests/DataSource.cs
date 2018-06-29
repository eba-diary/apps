﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// Base class for any type of retriever data source
    /// This class is mapped into the database utilizing the table per hierarchy strategy with SourceType as the discriminator value.
    /// http://nhibernate.info/doc/nhibernate-reference/inheritance.html
    /// </summary
    /// 

    // TODO: Remove Interface
    public abstract class DataSource : IDataSource
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

        public abstract Uri CalcRelativeUri(RetrieverJob Job);
        public abstract string GetDropPrefix(RetrieverJob Job);

        //https://stackoverflow.com/questions/2664245/identifying-nhibernate-proxy-classes
        //http://sessionfactory.blogspot.com/2010/08/hacking-lazy-loaded-inheritance.html
        public virtual Boolean Is<T>() where T : DataSource
        {
            return (this is T) ? true : false;
        }
    }
}