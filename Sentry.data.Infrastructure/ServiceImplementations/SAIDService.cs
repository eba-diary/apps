using Sentry.Common.Logging;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Interfaces;
using Sentry.data.Core.Interfaces.SAIDRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.ServiceImplementations
{
    public class SAIDService : ISAIDService
    {
        private readonly IAssetClient _assetClient;
        private readonly ObjectCache cache = MemoryCache.Default;
        private readonly SemaphoreSlim SAIDAssetListLock = new SemaphoreSlim(1);

        public SAIDService(IAssetClient assetClient)
        {
            _assetClient = assetClient;
        }

        public async Task<SAIDAsset> GetAssetByKeyCodeAsync(string keyCode)
        {
            SlimAssetEntity fromAsset = await _assetClient.GetAssetByKeyCodeAsync(keyCode, true).ConfigureAwait(false);

            SAIDAsset asset = ToEntity(fromAsset);

            return asset;
        }

        /// <summary>
        /// Gets all Production Custodians for a given SAID asset.
        /// </summary>
        /// <param name="keyCode">SAID Asset Keycode</param>
        /// <returns></returns>
        public async Task<List<SAIDRole>> GetApproversByKeyCodeAsync(string keyCode)
        {
            SAIDAsset asset = await GetAssetByKeyCodeAsync(keyCode).ConfigureAwait(false);

            //STEP 1: FIND ALL CUSTODIAN_CERTIFIER
            List<SAIDRole> approvers = asset.Roles.Where(w => w.Role.Equals(Sentry.data.Core.GlobalConstants.SAIDRoles.CUSTODIAN_CERTIFIER)).ToList();

            //STEP 2: IF NO CUSTODIAN_CERTIFIER FOUND, THEN BACKUP IS CUSTODIAN_PRODUCTION
            if (approvers.Count == 0)
            {
                approvers.AddRange(asset.Roles.Where(w => w.Role.Equals(Sentry.data.Core.GlobalConstants.SAIDRoles.CUSTODIAN_PRODUCTION)).ToList());
            }
            
            return approvers;
        }

        public async Task<List<SAIDAsset>> GetAllAssetsAsync()
        {
            /* We are caching the list of all SAID assets for two reasons:
             *  1.) The call to retrieve the list takes longer than we would like
             *  2.) SAID assets do not change frequently
             */
            List<SAIDAsset> resultContents = cache["SAIDAssets"] as List<SAIDAsset>;

            /*
             * Typcial lock method (https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/lock-statement) does not allow async await calls  
             * within body of lock statment, therefore, using a techique with SemaphoreSlim class 
             * (https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming#know-your-tools)
             */
            await SAIDAssetListLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (resultContents == null)
                {
                    //Get expiration configuration, set default if necessary
                    string configEntry = Configuration.Config.GetHostSetting("SaidCacheExpiration");
                    if (String.IsNullOrWhiteSpace(configEntry))
                    {
                        Logger.Debug($"SaidCacheExpiration not found: using default of 2 hours");
                        configEntry = "2,0,0";
                    }
                    int[] cacheExpiration = configEntry.Split(',').Select(Int32.Parse).ToArray();

                    //Create cache policy and set expiration policy based on config
                    CacheItemPolicy policy = new CacheItemPolicy();

                    //SlidingExpriration will restart the experation timer when the cache is accessed, if it has not already expired.
                    policy.SlidingExpiration = new System.TimeSpan(cacheExpiration[0], cacheExpiration[1], cacheExpiration[2]);

                    //Pull list from Rest client
                    ICollection<SlimAssetEntity> listFromAsset = await _assetClient.GetAllAssetsAsync().ConfigureAwait(false);

                    //Convert API objects to DSC Entity objects
                    List<SAIDAsset> assetList = new List<SAIDAsset>();
                    foreach (SlimAssetEntity fromAsset in listFromAsset)
                    {
                        assetList.Add(ToEntity(fromAsset));
                    }
                                        
                    resultContents = assetList;

                    //Assign result list to cache object
                    cache.Set("SAIDAssets", resultContents, policy);
                }

                //Return object from cache
                return resultContents;
            }
            finally
            {
                SAIDAssetListLock.Release();
            }            
        }

        public async Task<bool> VerifyAssetExistsAsync(string keyCode)
        {
            try
            {
                await _assetClient.GetAssetByKeyCodeAsync(keyCode, false);
                return true;
            }
            catch (ApiException)
            {
                return false;
            }
        }

        #region Private
        private SAIDAsset ToEntity(SlimAssetEntity fromAsset)
        {
            SAIDAsset toAsset = new SAIDAsset()
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

            if (fromAsset.RoleList != null)
            {
                foreach (SlimRoleEntity fromRole in fromAsset.RoleList)
                {
                    toAsset.Roles.Add(ToEntity(fromRole));
                }
            }

            return toAsset;
        }

        private SAIDRole ToEntity(SlimRoleEntity fromRole)
        {
            SAIDRole toRole = new SAIDRole()
            {
                Role = fromRole.Role,
                Name = fromRole.Name,
                AssociateId = fromRole.AssociateId
            };

            return toRole;
        }

        #endregion
    }
}
