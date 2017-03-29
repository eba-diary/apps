using System;
using static Sentry.Common.SystemClock;

namespace Sentry.data.Core
{
    /// <summary>
    /// This is a domain-level User type.  The rest of the domain model may have references to
    /// objects of this type.  This object only contains information that our particular domain model
    /// deals with directly (that is, not information from other sources such as HR or Obsidian).  The
    /// IApplicationUser interface represents a more holistic/aggregated view of a user.
    /// </summary>
    /// <remarks></remarks>
    public class DomainUser
    {
#pragma warning disable CS0649
        private int _id;
        private int _version;
#pragma warning restore CS0649
        private string _associateId;

        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        private int _ranking;
        private DateTime _created;

        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
        protected DomainUser()
        {

        }
        public DomainUser(string associateId)
        {
            _associateId = associateId;
            //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
            _created = Now();
            //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
        }

        public virtual int Id
        {
            get
            {
                return _id;
            }
        }
        public virtual string AssociateId
        {
            get
            {
                return _associateId;
            }
        }

        // ###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        public virtual int Version
        {
            get
            {
                return _version;
            }
        }

        public virtual DateTime Created
        {
            get
            {
                return _created;
            }
        }

        public virtual int Ranking
        {
            get
            {
                return _ranking;
            }
            set
            {
                _ranking = value;
            }
        }

        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }
}
