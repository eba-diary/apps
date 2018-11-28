using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    [Serializable]
    public class DatasetMetadata
    {
        private string _reportMetadata;
        
        public virtual ReportMetadata ReportMetadata
        {
            get
            {
                if (String.IsNullOrEmpty(_reportMetadata))
                {
                    return null;
                }
                else
                {
                    ReportMetadata a = JsonConvert.DeserializeObject<ReportMetadata>(_reportMetadata);
                    return a;
                }
            }
            set
            {
                _reportMetadata = JsonConvert.SerializeObject(value);
            }
        }

    }
}
