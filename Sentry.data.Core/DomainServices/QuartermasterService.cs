using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces.QuartermasterRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// Domain Service responsible for interactions with Quartermaster
    /// </summary>
    public class QuartermasterService : IQuartermasterService
    {
        private readonly IClient _quartermasterClient;
        private readonly IDataFeatures _dataFeatures;

        public QuartermasterService(IClient quartermasterClient, IDataFeatures dataFeatures)
        {
            _quartermasterClient = quartermasterClient;
            _dataFeatures = dataFeatures;
        }

        /// <summary>
        /// If the asset is managed in Quartermaster, then the named environment and named environment type must be valid according to Quartermaster
        /// </summary>
        /// <param name="saidAssetKeyCode">The 4-digit SAID asset key code</param>
        /// <param name="namedEnvironment">The named environment to validate</param>
        /// <param name="namedEnvironmentType">The named environment type to validate</param>
        /// <returns>A list of ValidationResults. These should be merged into any existing ValidationResults.</returns>
        public async Task<ValidationResults> VerifyNamedEnvironmentAsync(string saidAssetKeyCode, string namedEnvironment, NamedEnvironmentType namedEnvironmentType)
        {
            var results = new ValidationResults();
            //validate parameters
            if (string.IsNullOrWhiteSpace(saidAssetKeyCode))
            {
                results.Add(GlobalConstants.ValidationErrors.SAID_ASSET_REQUIRED, "SAID Asset Key Code is required.");
            }
            if (string.IsNullOrWhiteSpace(namedEnvironment))
            {
                results.Add(GlobalConstants.ValidationErrors.NAMED_ENVIRONMENT_INVALID, "Named Environment is required.");
            }

            if (results.GetAll().Count == 0)
            {
                var namedEnvironmentList = (await GetNamedEnvironmentsAsync(saidAssetKeyCode)).ToList();
                if (namedEnvironmentList.Any())
                {
                    if (!namedEnvironmentList.Any(e => e.NamedEnvironment == namedEnvironment))
                    {
                        results.Add(GlobalConstants.ValidationErrors.NAMED_ENVIRONMENT_INVALID, $"Named Environment provided (\"{namedEnvironment}\") doesn't match a Quartermaster Named Environment for asset {saidAssetKeyCode}.");
                    }
                    else if (namedEnvironmentList.First(e => e.NamedEnvironment == namedEnvironment).NamedEnvironmentType != namedEnvironmentType)
                    {
                        var quarterMasterNamedEnvironmentType = namedEnvironmentList.First(e => e.NamedEnvironment == namedEnvironment).NamedEnvironmentType;
                        results.Add(GlobalConstants.ValidationErrors.NAMED_ENVIRONMENT_TYPE_INVALID, $"Named Environment Type provided (\"{namedEnvironmentType}\") doesn't match Quartermaster (\"{quarterMasterNamedEnvironmentType}\")");
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Given a SAID asset key code, get all the named environments from Quartermaster
        /// </summary>
        /// <param name="saidAssetKeyCode">The four-character key code for an asset</param>
        /// <returns>A list of NamedEnvironmentDto objects</returns>
        public Task<List<NamedEnvironmentDto>> GetNamedEnvironmentsAsync(string saidAssetKeyCode)
        {
            //validate parameters
            if (string.IsNullOrWhiteSpace(saidAssetKeyCode))
            {
                throw new ArgumentNullException(nameof(saidAssetKeyCode), "SAID Asset Key Code was missing.");
            }

            //the guts of the method have to be wrapped in a local function for proper async handling
            //see https://confluence.sentry.com/questions/224368523
            async Task<List<NamedEnvironmentDto>> GetNamedEnvironmentsInternalAsync()
            {
                //call Quartermaster to get list of named environments for this asset
                var namedEnvironmentList = (await _quartermasterClient.NamedEnvironmentsGet2Async(saidAssetKeyCode, ShowDeleted10.False)).ToList();
                namedEnvironmentList = namedEnvironmentList.OrderBy(n => n.Name).ToList();

                //grab a feature flag to see if we need to filter the named environments by a certain named environment type
                //if the feature flag setting for "CLA4260_QuartermasterNamedEnvironmentTypeFilter" is blank, no filter will be applied
                var environmentTypeFilter = _dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue();
                Func<NamedEnvironment, bool> filter = env => true;
                if (!string.IsNullOrWhiteSpace(environmentTypeFilter))
                {
                    filter = env => env.Environmenttype == environmentTypeFilter;
                }

                //map the output from Quartermaster to our Dto
                return namedEnvironmentList.Where(filter).Select(env => new NamedEnvironmentDto
                {
                    NamedEnvironment = env.Name,
                    NamedEnvironmentType = (GlobalEnums.NamedEnvironmentType)Enum.Parse(typeof(GlobalEnums.NamedEnvironmentType), env.Environmenttype)
                }).ToList();
            }
            return GetNamedEnvironmentsInternalAsync();
        }
    }
}
