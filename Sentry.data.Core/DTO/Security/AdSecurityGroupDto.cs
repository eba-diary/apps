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
        private AdSecurityGroupDto(string saidAssetCode, string datasetShortName, AdSecurityGroupTypeEnum groupType, AdSecurityGroupEnvironmentTypeEnum environmentType)
        {
            SaidAssetCode = saidAssetCode;
            DatasetShortName = datasetShortName;
            GroupType = groupType;
            EnvironmentType = environmentType;
        }

        /// <summary>
        /// Static method for creating a "Dataset"-level group
        /// </summary>
        public static AdSecurityGroupDto NewDatasetGroup(string saidAssetCode, string datasetShortName, AdSecurityGroupTypeEnum groupType, AdSecurityGroupEnvironmentTypeEnum environmentType)
        {
            return new AdSecurityGroupDto(saidAssetCode, datasetShortName, groupType, environmentType);
        }

        public static AdSecurityGroupDto NewAssetGroup(string saidAssetCode, string datasetShortName, AdSecurityGroupTypeEnum groupType, AdSecurityGroupEnvironmentTypeEnum environmentType)
        {
            return new AdSecurityGroupDto(saidAssetCode, SecurityConstants.ASSET_LEVEL_GROUP_NAME, groupType, environmentType);
        }


        public string SaidAssetCode { get; }
        public string DatasetShortName { get; }
        public AdSecurityGroupTypeEnum GroupType { get; }
        public AdSecurityGroupEnvironmentTypeEnum EnvironmentType { get; }

        public bool IsAssetLevelGroup() => DatasetShortName == SecurityConstants.ASSET_LEVEL_GROUP_NAME;

        public string GetGroupName()
        {
            return $"DS_{SaidAssetCode}_{DatasetShortName}_{GroupType}_{EnvironmentType}";
        }
    }
}
