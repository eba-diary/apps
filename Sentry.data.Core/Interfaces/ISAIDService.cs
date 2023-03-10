using Sentry.data.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces
{
    public interface ISAIDService
    {
        Task<SAIDAsset> GetAssetByKeyCodeAsync(string keyCode);
        Task<List<SAIDAsset>> GetAllAssetsAsync();
        Task<List<SAIDRole>> GetAllProdCustByKeyCodeAsync(string keyCode);
        Task<bool> VerifyAssetExistsAsync(string keyCode);
    }
}
