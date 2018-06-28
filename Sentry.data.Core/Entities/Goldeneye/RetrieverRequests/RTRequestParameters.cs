using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RTRequestParameters
    {
        private int _id;
        private int _requestId;
        private int _apiParameterId;
        private RTAPIParameters _apiParameter;
        private string _value;

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

        public virtual int RequestId
        {
            get
            {
                return _requestId;
            }

            set
            {
                _requestId = value;
            }
        }

        public virtual int ApiParameterId
        {
            get
            {
                return _apiParameterId;
            }

            set
            {
                _apiParameterId = value;
            }
        }

        public virtual RTAPIParameters ApiParameter
        {
            get
            {
                return _apiParameter;
            }

            set
            {
                _apiParameter = value;
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
    }
}
