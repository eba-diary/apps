using System;
using System.Net;

namespace Sentry.data.Core
{
    /// <summary>
    /// Base class for authentication types utilized by any data source type
    /// This class is mapped into the database utilizing the table per hierarchy strategy with AuthType being the discriminator value.
    /// http://nhibernate.info/doc/nhibernate-reference/inheritance.html
    /// </summary
    /// 
    public abstract class AuthenticationType : IAuthenticationType
    {
        public virtual int AuthID { get; set; }

        /// <summary>
        /// Discriminator value for nhibernate mapping
        /// </summary>
        public virtual string AuthType { get; set; }
        public virtual string AuthName { get; set; }
        public virtual string Description { get; set; }
        public virtual bool IsUserPassRequired { get; set; }
        public abstract NetworkCredential GetCredentials(RetrieverJob Job);
        public virtual Boolean Is<T>() where T : AuthenticationType
        {
            return (this is T) ? true : false;
        }
    }
    
}
