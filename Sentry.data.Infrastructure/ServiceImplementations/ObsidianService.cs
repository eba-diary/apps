using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core;
using Sentry.Web.Security.Obsidian;

namespace Sentry.data.Infrastructure
{
    public class ObsidianService : IObsidianService
    {

        private ObsidianAdService _service;

        public ObsidianService() { }

        public ObsidianAdService Service()
        {
            if(_service == null)
            {
                _service = new ObsidianAdService()
                {
                    UseDefaultCredentials = true,
                    Url = Configuration.Config.GetHostSetting("ObsidianAdServiceUrl")
                };
            }
            return _service;
        }

        public bool DoesGroupExist(string adGroup)
        {
            cdtGetGroupListRequest request = new cdtGetGroupListRequest()
            {
                LogicalDomainName = "Intranet",
                GroupNameQuery = adGroup
            };
            cdtGetGroupListResponse response = Service().GetGroupList(request);

            if(response?.GroupNames == null) { return false; }
            return response.GroupNames.ToUpper() == adGroup.ToUpper();
        }

        public List<string> GetAdGroups(string adGroup)
        {
            cdtGetGroupListRequest request = new cdtGetGroupListRequest()
            {
                LogicalDomainName = "Intranet",
                GroupNameQuery = adGroup + "*"
            };
            cdtGetGroupListResponse response = Service().GetGroupList(request);

            if (response?.GroupNames == null) { return new List<string>(); }
            return response.GroupNames.Split(',').OrderBy(x=> x).ToList();
        }

        public void GetUsersInGroup(string groupName)
        {
            cdtGetUsersByGroupRequest request = new cdtGetUsersByGroupRequest()
            {
                LogicalDomainName = "Intranet",
                GroupNameQuery = groupName,
                PropertyNameList = new List<string>() { "UserFullName", "UserEmployeeId"}
            };

            cdtGetUsersByGroupResponse response = Service().GetUsersByGroup(request);

            var t = response;
        }

    }
}
