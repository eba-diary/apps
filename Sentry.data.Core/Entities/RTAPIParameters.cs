using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RTAPIParameters
    {
        private int _id;
        private int _sourceTypeId;
        private int _apiEndpointId;
        private string _name;

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

        public virtual int ApiEndpointId
        {
            get
            {
                return _apiEndpointId;
            }

            set
            {
                _apiEndpointId = value;
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
    }
}
