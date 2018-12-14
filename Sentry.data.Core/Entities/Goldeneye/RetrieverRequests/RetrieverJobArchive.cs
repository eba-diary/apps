using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class RetrieverJobArchive
    {

        private string _jobOptions;

        public RetrieverJobArchive()
        {
        }

        public virtual int Archive_Id { get; set; }
        public virtual RetrieverJob RetrieverJob { get; set; }
        public virtual Guid Job_Guid { get; set; }
        public virtual string Schedule { get; set; }
        public virtual string RelativeUri { get; set; }
        public virtual DataSource DataSource { get; set; }
        public virtual DatasetFileConfig DatasetConfig { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
        public virtual Boolean IsGeneric { get; set; }
        public virtual Boolean IsEnabled { get; set; }

        //Property is stored within database as string
        public virtual RetrieverJobOptions JobOptions
        {
            get
            {
                if (String.IsNullOrEmpty(_jobOptions))
                {
                    return null;
                }
                else
                {
                    RetrieverJobOptions a = JsonConvert.DeserializeObject<RetrieverJobOptions>(_jobOptions);
                    return a;
                }
            }
            set
            {
                _jobOptions = JsonConvert.SerializeObject(value);
            }
        }
    }
}
