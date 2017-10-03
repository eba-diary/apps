using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.DatasetLoader.Entities
{
    class SystemConfig
    {
        public string systemName { get; set;}
        public List<FileConfig> fileConfigs { get; set; }
    }
}
