using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetFolder
    {
        private string _bucket;
        private string _key;
        private string _eTag;
        private string _name;
        private string _description;
        private DatasetFolder _parentFolder;
        private IList<DatasetFolder> _subFolders = new List<DatasetFolder>();
        private IList<Dataset> _dataSets = new List<Dataset>();
        private Dictionary<string, string> _metadata;

        protected DatasetFolder()
        {

        }

        public DatasetFolder(string bucket, string key, string eTag, string name, DatasetFolder parentFolder = null)
        {
            _bucket = bucket;
            _name = name;
            _key = key;
            _eTag = eTag;
            this.ParentFolder = parentFolder;
        }

        public virtual string Bucket
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

        public virtual string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }

        public virtual string ETag
        {
            get
            {
                return _eTag;
            }
            set
            {
                _eTag = value;
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

        public virtual DatasetFolder ParentFolder
        {
            get
            {
                return _parentFolder;
            }
            set
            {
                //If the current parent is set, remove the child (me) from it
                if (_parentFolder != null)
                {
                    _parentFolder._subFolders.Remove(this);
                }
                _parentFolder = value;
                //Now, add me to the children of the new parent
                if (_parentFolder != null)
                {
                    _parentFolder._subFolders.Add(this);
                }
            }
        }

        public virtual IList<DatasetFolder> SubFolders
        {
            get
            {
                return _subFolders;
            }
        }

        public virtual string FullName
        {
            get
            {
                return this.ToString();
            }
        }

        public virtual IList<Dataset> Datasets
        {
            get
            {
                return _dataSets;
            }
        }

        public virtual Dictionary<string, string> Metadata
        {
            get
            {
                return _metadata;
            }
            set
            {
                _metadata = value;
            }
        }

        public override string ToString()
        {
            if (ParentFolder != null)
            {
                return ParentFolder.ToString() + " > " + Name;
            }
            else
            {
                return Name;
            }
        }
    }
}
