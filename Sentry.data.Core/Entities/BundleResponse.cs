using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BundleResponse
    {
        private string _requestGuid;
        private int _datasetId;
        private int _datasetFileConfigId;
        private string _targetBucket;
        private string _targetKey;
        private string _targetVersionId;
        private string _targetETag;

        public string RequestGuid
        {
            get
            {
                return _requestGuid;
            }
            set
            {
                _requestGuid = value;
            }
        }
        public int DatasetID
        {
            get
            {
                return _datasetId;
            }
            set
            {
                _datasetId = value;
            }
        }
        public int DatasetFileConfigId
        {
            get
            {
                return _datasetFileConfigId;
            }
            set
            {
                _datasetFileConfigId = value;
            }
        }
        public string TargetBucket
        {
            get
            {
                return _targetBucket;
            }
            set
            {
                _targetBucket = value;
            }
        }
        public string TargetKey
        {
            get
            {
                return _targetKey;
            }
            set
            {
                _targetKey = value;
            }
        }
        public string TargetVersionId
        {
            get
            {
                return _targetVersionId;
            }
            set
            {
                _targetVersionId = value;
            }
        }
        public string TargetETag
        {
            get
            {
                return _targetETag;
            }
            set
            {
                _targetETag = value;
            }
        }
    }
}
