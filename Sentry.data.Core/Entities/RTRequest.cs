using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RTRequest
    {
        private int _id;
        private int _sourceTypeId;
        private int _endpointId;
        private int _requestorId;
        private string _systemFolder;
        private Boolean _enabled;
        private string _requestName;
        private RTAPIEndpoints _endpoint;
        private RTSourceTypes _sourceType;
        private IList<RTRequestParameters> _parameters;

        public RTRequest()
        {
            Parameters = new List<RTRequestParameters>();
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

        public virtual RTSourceTypes SourceType
        {
            get
            {
                return _sourceType;
            }

            set
            {
                _sourceType = value;
            }
        }

        public virtual int EndpointId
        {
            get
            {
                return _endpointId;
            }

            set
            {
                _endpointId = value;
            }
        }

        public virtual RTAPIEndpoints Endpoint
        {
            get
            {
                return _endpoint;
            }

            set
            {
                _endpoint = value;
            }
        }

        public virtual int RequestorId
        {
            get
            {
                return _requestorId;
            }

            set
            {
                _requestorId = value;
            }
        }

        public virtual IList<RTRequestParameters> Parameters
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

        public virtual Boolean IsEnabled
        {
            get
            {
                return (bool)_enabled;
            }

            set
            {
                _enabled = (bool)value;
            }
        }

        public virtual string SystemFolder
        {
            get
            {
                return _systemFolder;
            }

            set
            {
                _systemFolder = value;
            }
        }

        public virtual string RequestName
        {
            get
            {
                return _requestName;
            }

            set
            {
                _requestName = value;
            }
        }
    }
}
