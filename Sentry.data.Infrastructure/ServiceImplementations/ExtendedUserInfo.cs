using System;
using System.Collections.Generic;
using Sentry.Associates;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    /// <summary>
    /// An implementation of IExtendedUserInfo that specifically deals with information from Obsidian
    /// (a Sentry.Web.Security.IUser instance) and from HR (a Sentry.Associates.Associate instance)
    /// </summary>
    /// <remarks></remarks>
    public class ExtendedUserInfo : IExtendedUserInfo
    {
        private string _userId;

        //Note the use of "Lazy" here... This is so that the information from HR and Obsidian is lazy-loaded
        //only if required as part of a particular request for data.
        private Lazy<Associate> _associateInfo;
        private Lazy<Sentry.Web.Security.IUser> _obsidianUser;


        public ExtendedUserInfo(string userId, Lazy<Sentry.Web.Security.IUser> obsidianUser, Lazy<Associate> associateInfo)
        {
            _userId = userId;
            _associateInfo = associateInfo;
            _obsidianUser = obsidianUser;
        }

        public string EmailAddress
        {
            get
            {
                return _associateInfo.Value.WorkEmailAddress;
            }
        }
        public string FamiliarName
        {
            get
            {
                return _associateInfo.Value.FamiliarName;
            }
        }

        public string FirstName
        {
            get
            {
                return _associateInfo.Value.FirstName;
            }
        }


        public string LastName
        {
            get
            {
                return _associateInfo.Value.LastName;
            }
        }

        public Boolean IsInGroup(string groupName)
        {
            return _obsidianUser.Value.IsInGroup(groupName);
        }

        public IEnumerable<string> Permissions
        {
            get
            {
                return _obsidianUser.Value.GetPermissions();
            }
        }
        public string UserId
        {
            get
            {
                return _userId;
            }
        }
    }
}
