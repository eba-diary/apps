using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DataAsset : ISecurable
    {

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
    }
}
