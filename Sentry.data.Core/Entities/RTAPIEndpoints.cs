using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RTAPIEndpoints
    {
        private int _id;
        private int _sourceTypeId;
        private string _name;
        private string _value;
        private IList<RTAPIParameters> _parameters;

        public RTAPIEndpoints()
        {
            Parameters = new List<RTAPIParameters>();
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

        public virtual int SourceTypeId
        {
            get
            {
                return _sourceTypeId;
            }

            set
            {
                _sourceTypeId = value;
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
        
        public virtual string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public virtual IList<RTAPIParameters> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                _parameters = value;
            }
        }
    }
}
