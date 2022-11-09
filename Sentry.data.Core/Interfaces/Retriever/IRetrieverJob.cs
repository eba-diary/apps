using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Core
{
    public interface IRetrieverJob
    {
        int Id { get; set; }
        string Schedule { get; set; }
        string RelativeUri { get; set; }
        DataSource DataSource { get; set; }
        DatasetFileConfig DatasetConfig { get; set; }
        bool IsGeneric { get; set; }
    }
}
