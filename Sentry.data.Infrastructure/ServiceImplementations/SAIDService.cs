using Sentry.data.Core.Entities;
using Sentry.data.Core.Interfaces;
using Sentry.data.Core.Interfaces.SAIDRestClient;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.ServiceImplementations
{
    public class SAIDService : ISAIDService
    {
        private readonly IAssetClient _assetClient;
        
        public SAIDService(IAssetClient assetClient)
        {
            _assetClient = assetClient;
        }

        public async Task<SAIDAsset> GatAssetByKeyCode(string keyCode)
        {
            SlimAssetEntity fromAsset = await _assetClient.GetAssetByKeyCodeAsync(keyCode, true).ConfigureAwait(false);

            SAIDAsset asset = new SAIDAsset()
            {
                Id = fromAsset.Id,
                Name = fromAsset.Name,
                SaidKeyCode = fromAsset.SaidKeyCode,
                SosTeamId = fromAsset.SosTeamId,
                AssetOfRisk = fromAsset.AssetOfRisk,
                DataClassification = fromAsset.DataClassification,
                SystemIsolation = fromAsset.SystemIsolation,
                ChargeableUnit = fromAsset.ChargeableUnit,
                AwsLocation = fromAsset.AwsLocation
            };

            foreach (SlimRoleEntity fromRole in fromAsset.RoleList)
            {
                asset.Roles.Add(new SAIDRole()
                {
                    Role = fromRole.Role,
                    Name = fromRole.Name,
                    AssociateId = fromRole.AssociateId
                });
            }

            return asset;
        }
    }
}
