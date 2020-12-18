using Sentry.data.Core.Entities;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces
{
    public interface ISAIDService
    {
        Task<SAIDAsset> GatAssetByKeyCode(string keyCode);
    }
}
