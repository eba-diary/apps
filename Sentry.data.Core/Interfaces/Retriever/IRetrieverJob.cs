using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sentry.data.Core
{
    public interface IRetrieverJob
    {
        int Id { get; set; }
        string Schedule { get; set; }
        string RelativeUri { get; set; }
        DataSource DataSource { get; set; }
        DatasetFileConfig DatasetConfig { get; set; }
        Boolean IsGeneric { get; set; }
        Uri GetUri();
    }
}
