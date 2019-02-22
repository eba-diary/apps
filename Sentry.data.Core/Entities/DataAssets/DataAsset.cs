using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DataAsset : ISecurable
    {
        private List<DataAssetHealth> _assetHealth;

        public DataAsset()
        {
            Components = new List<ConsumptionLayerComponent>();
        }


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
        public virtual string MetadataRepAssetName { get; set; }
        public virtual IList<AssetSource> AssetSource{ get; set;}
        public virtual IList<AssetNotifications> AssetNotifications { get; set; }

        //ISecurable
        public virtual bool IsSecured { get; set; }
        public virtual string PrimaryOwnerId { get; set; }
        public virtual string PrimaryContactId { get; set; }
        public virtual Security Security { get; set; }




        public virtual List<DataAssetHealth> AssetHealth
        {
            get
            {
                if (_assetHealth == null)
                {
                    _assetHealth = MetadataRepositoryService.GetByAssetName(MetadataRepAssetName, AssetSource.Where(w => w.IsVisiable).ToList()).OrderBy(o => o.SourceSystem).ToList();
                }
                return _assetHealth;
            }
        }
        public virtual Boolean HealthIncludesSourceSystems
        {
            get
            {
                if (this.AssetHealth.Count > 1 || (this.AssetHealth.Count == 1 && this.AssetHealth.FirstOrDefault().SourceSystem != ""))
                {
                    return true;
                }
                return false;
            }
        }
        public virtual DateTime MaxProcessTime
        {
            get
            {
                if (this.AssetHealth.Count > 0)
                {
                    return this.AssetHealth.Max(m => m.AssetUpdtDTM);
                }
                return DateTime.MinValue;
            }
        }

    }
}
