using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IQuartermasterService
    {
        /// <summary>
        /// If the asset is managed in Quartermaster, then the named environment and named environment type must be valid according to Quartermaster
        /// </summary>
        /// <param name="saidAssetKeyCode">The 4-digit SAID asset key code</param>
        /// <param name="namedEnvironment">The named environment to validate</param>
        /// <param name="namedEnvironmentType">The named environment type to validate</param>
        /// <returns>A list of ValidationResults. These should be merged into any existing ValidationResults.</returns>
        Task<ValidationResults> VerifyNamedEnvironmentAsync(string saidAssetKeyCode, string namedEnvironment, NamedEnvironmentType namedEnvironmentType);

        /// <summary>
        /// Given a SAID asset key code, get all the named environments from Quartermaster
        /// </summary>
        /// <param name="saidAssetKeyCode">The four-character key code for an asset</param>
        /// <returns>A list of NamedEnvironmentDto objects</returns>
        Task<List<NamedEnvironmentDto>> GetNamedEnvironmentsAsync(string saidAssetKeyCode);
    }
}
