using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetFileConfig
    {
#pragma warning disable CS0649
        private int _configId;
#pragma warning restore CS0649
        private string _name;
        private string _description;
        private int _dataFileConfigId;
        private int _datasetId;
        private string _searchCriteria;
        private string _targetFileName;
        private string _dropLocationType;
        private string _dropPath;
        private Boolean _isRegexSearch;
        private Boolean _overwriteDatafile;
        private int _versionsToKeep;
        private int _fileTypeId;
        private Boolean _isGeneric;

        public DatasetFileConfig() { }

        public DatasetFileConfig(
            int configId,
            string name,
            string description,
            int dataFileConfigId,
            int datasetId,
            string searchCriteria,
            string dropLocationType,
            string dropPath,
            Boolean isRegexSearch,
            Boolean overwriteDatafile,
            int versionsToKeep,
            int fileTypeId,
            Boolean isGeneric)
        {
            this._configId = configId;
            this._name = name;
            this._description = description;
            this._dataFileConfigId = dataFileConfigId;
            this._datasetId = datasetId;
            this._searchCriteria = searchCriteria;
            this._dropLocationType = dropLocationType;
            this._dropPath = dropPath;
            this._isRegexSearch = isRegexSearch;
            this._overwriteDatafile = overwriteDatafile;
            this._versionsToKeep = versionsToKeep;
            this._fileTypeId = fileTypeId;
            this._isGeneric = isGeneric;
        }
        
        public virtual int ConfigId
        {
            get
            {
                return _configId;
            }
            set
            {
                _configId = value;
            }
        }
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public virtual string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }
        public virtual int DataFileConfigId
        {
            get
            {
                return _dataFileConfigId;
            }
            set
            {
                _dataFileConfigId = value;
            }
        }
        public virtual int DatasetId
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
        public virtual string SearchCriteria
        {
            get
            {
                return _searchCriteria;
            }
            set
            {
                _searchCriteria = value;
            }
        }
        public virtual string TargetFileName
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
        public virtual string DropLocationType
        {
            get
            {
                return _dropLocationType;
            }
            set
            {
                _dropLocationType = value;
            }
        }
        public virtual string DropPath
        {
            get
            {
                return _dropPath;
            }
            set
            {
                _dropPath = value;
            }
        }
        public virtual Boolean IsRegexSearch
        {
            get
            {
                return _isRegexSearch;
            }
            set
            {
                _isRegexSearch = value;
            }
        }
        public virtual Boolean OverwriteDatafile
        {
            get
            {
                return _overwriteDatafile;
            }
            set
            {
                _overwriteDatafile = value;
            }
        }
        public virtual int VersionsToKeep
        {
            get
            {
                return _versionsToKeep;
            }
            set
            {
                _versionsToKeep = value;
            }
        }
        public virtual int FileTypeId
        {
            get
            {
                return _fileTypeId;
            }
            set
            {
                _fileTypeId = value;
            }
        }

        public virtual Boolean IsGeneric
        {
            get
            {
                return _isGeneric;
            }
            set
            {
                _isGeneric = value;
            }
        }
    }
}
