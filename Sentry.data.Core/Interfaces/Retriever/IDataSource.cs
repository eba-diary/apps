using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using System.IO;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public interface IDataSource
    {
        int Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string SourceType { get; set; }
        AuthenticationType SourceAuthType { get; set; }
        string KeyCode { get; set; }
        Boolean IsUriEditable { get; set; }
        bool IsUserPassRequired { get; set; }
        bool IsSourceCompressed { get; set; }
        Uri CalcRelativeUri(RetrieverJob Job, NamedEnvironmentType datasetEnvironmentType, string CLA4260_QuartermasterNamedEnvironmentTypeFilter);
    }
}
