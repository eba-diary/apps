using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class ComponentElement
    {
        private int _id;
        private int _parentId;
        private int _cLC_Id;
        private string _name;
        private string _link;
        private int _status;
        private DateTime _lastUpdated;
        private IList<ComponentElement> _elements;

        public ComponentElement()
        {
            Elements = new List<ComponentElement>();
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

        public virtual string Link
        {
            get
            {
                return _link;
            }

            set
            {
                _link = value;
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

        public virtual IList<ComponentElement> Elements
        {
            get
            {
                return _elements;
            }

            set
            {
                _elements = value;
            }
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

        public virtual int ParentId
        {
            get
            {
                return _parentId;
            }

            set
            {
                _parentId = value;
            }
        }

        public virtual int CLC_Id
        {
            get
            {
                return _cLC_Id;
            }

            set
            {
                _cLC_Id = value;
            }
        }
    }
}
