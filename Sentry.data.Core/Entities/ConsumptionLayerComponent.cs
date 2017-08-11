using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class ConsumptionLayerComponent
    {
        private int _id;
        private int _dataAsset_Id;
        private int _type_Id;
        private ConsumptionLayerType type;
        private IList<ComponentElement> _componentElements;

        private int _status;
        private DateTime _lastUpdated;

        public ConsumptionLayerComponent()
        {
            ComponentElements = new List<ComponentElement>();
        }

        public virtual int Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
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

        public virtual IList<ComponentElement> ComponentElements
        {
            get
            {
                return _componentElements;
            }

            set
            {
                _componentElements = value;
            }
        }

        public virtual int DataAsset_Id
        {
            get
            {
                return _dataAsset_Id;
            }

            set
            {
                _dataAsset_Id = value;
            }
        }

        public virtual int Type_Id
        {
            get
            {
                return _type_Id;
            }

            set
            {
                _type_Id = value;
            }
        }

        public virtual ConsumptionLayerType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }
    }
}
