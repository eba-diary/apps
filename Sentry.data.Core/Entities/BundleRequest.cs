using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BundleRequest
    {
        private string _requestGuid;
        private int _datasetId;
        private int _datasetFileConfigId;
        private string _bucket;
        private string _sourceKeysFileLocation;
        private string _sourceKeysFileVersionId;
        private List<Tuple<string, string>> _sourceKeys;
        private string _fileExtension;
        private string _targetFileName;
        private string _email;
                
        public BundleRequest()
        {
            _requestGuid = Guid.NewGuid().ToString("D");
            _sourceKeys = new List<Tuple<string, string>>();
        }

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

        public string Bucket
        {
            get
            {
                return _bucket;
            }

            set
            {
                _bucket = value;
            }
        }

        public string SourceKeysFileLocation
        {
            get
            {
                return _sourceKeysFileLocation;
            }

            set
            {
                _sourceKeysFileLocation = value;
            }
        }

        public string SourceKeysFileVersionId
        {
            get
            {
                return _sourceKeysFileVersionId;
            }
            set
            {
                _sourceKeysFileVersionId = value;
            }
        }

        public List<Tuple<string, string>> SourceKeys
        {
            get
            {
                return _sourceKeys;
            }
            set
            {
                _sourceKeys = value;
            }
        }

        public string TargetFileName
        {
            get
            {
                return _targetFileName;
            }

            set
            {
                _targetFileName = value;
            }
        }

        public string FileExtension
        {
            get
            {
                return _fileExtension;
            }

            set
            {
                _fileExtension = value;
            }
        }

        public string Email
        {
            get
            {
                return _email;
            }

            set
            {
                _email = value;
            }
        }
    }
}
