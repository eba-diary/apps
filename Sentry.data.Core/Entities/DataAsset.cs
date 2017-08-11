using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataAsset
    {
        private int id;
        private string _name;
        private string _displayName;
        private string _archLink;
        private string _dataModelLink;
        private string _guideLink;
        private string _contact;
        private string _description;
        private IList<ConsumptionLayerComponent> _components;

        private DateTime _lastUpdated;
        private int _status;

        //private IList<string> _links;

        public DataAsset()
        {
            Components = new List<ConsumptionLayerComponent>();
        }

        public virtual int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
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

        public virtual string DisplayName
        {
            get
            {
                return _displayName;
            }

            set
            {
                _displayName = value;
            }
        }

        public virtual DateTime LastUpdated
        {
            get
            {
                return _lastUpdated;
            }

            set
            {
                _lastUpdated = value;
            }
        }

        public virtual int Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }

        public virtual IList<ConsumptionLayerComponent> Components
        {
            get
            {
                return _components;
            }

            set
            {
                _components = value;
            }
        }

        public virtual string ArchLink
        {
            get
            {
                return _archLink;
            }

            set
            {
                _archLink = value;
            }
        }

        public virtual string GuideLink
        {
            get
            {
                return _guideLink;
            }

            set
            {
                _guideLink = value;
            }
        }

        public virtual string DataModelLink
        {
            get
            {
                return _dataModelLink;
            }

            set
            {
                _dataModelLink = value;
            }
        }

        public virtual string Contact
        {
            get
            {
                return _contact;
            }

            set
            {
                _contact = value;
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
    }
}
