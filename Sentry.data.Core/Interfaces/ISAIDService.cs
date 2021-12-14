﻿using Sentry.data.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces
{
    public interface ISAIDService
    {
        Task<SAIDAsset> GetAssetByKeyCode(string keyCode);
        Task<List<SAIDAsset>> GetAllAssets();
        Task<SAIDRole> GetProdCustByKeyCode(string keyCode);
    }
}
