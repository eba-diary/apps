using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RTSourceTypes
    {
        private int _id;
        private string _name;
        private string _baseUrl;
        private string _description;
        private string _type;
        private IList<RTAPIEndpoints> _endpoints;

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

        public virtual string BaseUrl
        {
            get
            {
                return _baseUrl;
            }

            set
            {
                _baseUrl = value;
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
        
        public virtual string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public virtual IList<RTAPIEndpoints> Endpoints
        {
            get
            {
                return _endpoints;
            }

            set
            {
                _endpoints = value;
            }
        }
    }
}
