using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class AssetSource
    {
        private int _sourceId;
        private string _displayName;
        private string _description;
        private string _metadataRepositorySrcSysName;
        private int _dataAssetId;
        private Boolean _isVisiable;
        private DataAsset _parentDataAsset;

        public virtual DataAsset ParentDataAsset
        {
            get { return _parentDataAsset; }
            set { _parentDataAsset = value; }
        }
        public virtual Boolean IsVisiable
        {
            get { return _isVisiable; }
            set { _isVisiable = value; }
        }

        public virtual int DataAssetId
        {
            get { return _dataAssetId; }
            set { _dataAssetId = value; }
        }

        public virtual string MetadataRepositorySrcSysName
        {
            get { return _metadataRepositorySrcSysName; }
            set { _metadataRepositorySrcSysName = value; }
        }

        public virtual string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public virtual string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        public virtual int SourceId
        {
            get { return _sourceId; }
            set { _sourceId = value; }
        }

    }
}
