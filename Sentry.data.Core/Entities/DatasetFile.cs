using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetFile
    {
#pragma warning disable CS0649
        private int _datasetFileId;
#pragma warning restore CS0649
        private string _fileName;
        private Dataset _dataset;
        private DatasetFileConfig _datasetFileConfig;
        private string _uploadUserName;
        private DateTime _createDTM;
        private DateTime _modifiedDTM;
        private string _fileLocation;
        private string _s3Key;
        private int? _parentDatasetFileId;
        private string _versionId;
        private Boolean _isSensitive;

        protected DatasetFile()
        {
        }

        public DatasetFile(int datasetFileId,
            string fileName,
            Dataset dataset,
            DatasetFileConfig datasetFileConfig,
            string uploadUserName,
            string fileLocation,
            DateTime createDTM,
            DateTime modifiedDTM,
            int? parentDatasetFileId,
            string versionId)
        {
            this._datasetFileId = datasetFileId;
            this._fileName = fileName;
            this._dataset = dataset;
            this._datasetFileConfig = datasetFileConfig;
            this._uploadUserName = uploadUserName;
            this._fileLocation = fileLocation;
            this._createDTM = createDTM;
            this._modifiedDTM = modifiedDTM;
            this._parentDatasetFileId = parentDatasetFileId;
            this._versionId = versionId;
        }

        public virtual int DatasetFileId
        {
            get
            {
                return _datasetFileId;
            }
            set
            {
                _datasetFileId = value;
            }
        }

        public virtual string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        //public virtual int DatasetId
        //{
        //    get
        //    {
        //        return _datasetId;
        //    }
        //    set
        //    {
        //        _datasetId = value;
        //    }
        //}

        public virtual Dataset Dataset
        {
            get
            {
                return _dataset;
            }
            set
            {
                _dataset = value;
            }
        }

        public virtual DatasetFileConfig DatasetFileConfig
        {
            get
            {
                return _datasetFileConfig;
            }
            set
            {
                _datasetFileConfig = value;
            }
        }

        public virtual string UploadUserName
        {
            get
            {
                return _uploadUserName;
            }
            set
            {
                _uploadUserName = value;
            }
        }

        public virtual DateTime CreateDTM
        {
            get
            {
                return _createDTM;
            }
            set
            {
                _createDTM = value;
            }
        }

        public virtual DateTime ModifiedDTM
        {
            get
            {
                return _modifiedDTM;
            }
            set
            {
                _modifiedDTM = value;
            }
        }

        public virtual string FileLocation
        {
            get
            {
                return _fileLocation;
            }
            set
            {                 
                _fileLocation = value;
            }
        }

        public virtual int? ParentDatasetFileId
        {
            get
            {
                return _parentDatasetFileId;
            }
            set
            {
                _parentDatasetFileId = value;
            }
        }

        public virtual string VersionId
        {
            get
            {
                return _versionId;
            }
            set
            {
                _versionId = value;
            }
        }

        /// <summary>
        /// Full S3 key of DatasetFile
        /// </summary>
        public virtual string S3Key
        {
            get
            {
                return this._fileLocation + this.FileName;
            }
            set
            {
                _s3Key = this._fileLocation + this.FileName;
            }
        }
        public virtual Boolean IsSensitive
        {
            get
            {
                return _dataset.IsSensitive;
            }
        }
    }

}
