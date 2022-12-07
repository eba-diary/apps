using Sentry.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.DTO.Security
{
    /// <summary>
    /// DTO to interact with the <see cref="IAdSecurityAdminProvider"/>
    /// </summary>
    public class AdSecurityGroupDto
    {
        /// <summary>
        /// Private constructor for creating a new DTO
        /// </summary>
        private AdSecurityGroupDto(string saidAssetCode, string datasetShortName, AdSecurityGroupType groupType, AdSecurityGroupEnvironmentType environmentType)
        {
            SaidAssetCode = saidAssetCode;
            DatasetShortName = datasetShortName;
            GroupType = groupType;
            EnvironmentType = environmentType;
        }

        /// <summary>
        /// Static method for creating a "Dataset"-level group
        /// </summary>
        public static AdSecurityGroupDto NewDatasetGroup(string saidAssetCode, string datasetShortName, AdSecurityGroupType groupType, AdSecurityGroupEnvironmentType environmentType)
        {
            return new AdSecurityGroupDto(saidAssetCode, datasetShortName, groupType, environmentType);
        }

        public static AdSecurityGroupDto NewAssetGroup(string saidAssetCode, string datasetShortName, AdSecurityGroupType groupType, AdSecurityGroupEnvironmentType environmentType)
        {
            return new AdSecurityGroupDto(saidAssetCode, SecurityConstants.ASSET_LEVEL_GROUP_NAME, groupType, environmentType);
        }


        public string SaidAssetCode { get; }
        public string DatasetShortName { get; }
        public AdSecurityGroupType GroupType { get; }
        public AdSecurityGroupEnvironmentType EnvironmentType { get; }

        public bool IsAssetLevelGroup() => DatasetShortName == SecurityConstants.ASSET_LEVEL_GROUP_NAME;

        public string GetGroupName()
        {
            var env = Config.GetHostSetting("EnvironmentName");
            if (!string.Equals(env, "PROD")) //if nonprod, return add env to ticket 
            {
                return $"DS_{env}_{SaidAssetCode}_{DatasetShortName}_{GroupType}_{EnvironmentType}";
            }
            return $"DS_{SaidAssetCode}_{DatasetShortName}_{GroupType}_{EnvironmentType}";
        }
    }
}
