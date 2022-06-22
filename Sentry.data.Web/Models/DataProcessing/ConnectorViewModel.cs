using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class ConnectorViewModel
    {
        public int Id { get; set; }
        public IEnumerable<ConfluentConnectorRootModel> confluentConnectorRootModels { get; set; }
        public string SelectedConfluentConnectorRootModel { get; set; }
    }
}