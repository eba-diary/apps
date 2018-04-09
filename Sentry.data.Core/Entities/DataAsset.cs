﻿using Sentry.data.Core.Entities.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataAsset
    {
        private List<DataAssetHealth> _assetHealth;

        public DataAsset()
        {
            Components = new List<ConsumptionLayerComponent>();
        }

        public virtual List<DataElement> DataElements { get; set; }

        //ID From the Sentry Datasets Database
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual DateTime LastUpdated { get; set; }
        public virtual int Status { get; set; }
        public virtual IList<ConsumptionLayerComponent> Components { get; set; }
        public virtual string ArchLink { get; set; }
        public virtual string GuideLink { get; set; }
        public virtual string DataModelLink { get; set; }
        public virtual string Contact { get; set; }
        public virtual string Description { get; set; }
        public virtual IList<AssetSource> AssetSource{ get; set;}
        public virtual IList<AssetNotifications> AssetNotifications { get; set; }

        public virtual List<DataAssetHealth> AssetHealth
        {
            get
            {
                if (_assetHealth == null)
                {
                    _assetHealth = MetadataRepositoryService.GetByAssetName(DisplayName, AssetSource.Where(w => w.IsVisiable).ToList()).OrderBy(o => o.SourceSystem).ToList();
                    return _assetHealth;
                }
                else
                {
                    return _assetHealth;
                }
                
            }
        }
        public virtual Boolean HealthIncludesSourceSystems {
            get
            {                
                if (this.AssetHealth.Count > 1 || (this.AssetHealth.Count == 1 && this.AssetHealth.FirstOrDefault().SourceSystem != ""))
                {
                    return true;
                }
                else
                {
                    return false;
                }         
            }
        }
        public virtual DateTime MaxProcessTime {
            get
            {
                if (this.AssetHealth.Count > 0)
                {
                    if (this.AssetHealth.Count == 1)
                    {
                        return this.AssetHealth[0].AssetUpdtDTM;
                    }
                    else
                    {
                        return this.AssetHealth.Max(m => m.AssetUpdtDTM);
                    }                    
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }
    }
}
