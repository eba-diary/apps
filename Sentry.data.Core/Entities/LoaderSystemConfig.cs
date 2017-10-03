using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    class LoaderSystemConfig
    {
        public string systemName { get; set; }
        public List<LoaderFileConfig> fileConfigs { get; set; }
    }
}
