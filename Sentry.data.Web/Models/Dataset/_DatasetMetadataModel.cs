using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class _DatasetMetadataModel
    {
        public _DatasetMetadataModel()
        {
        }

        public _DatasetMetadataModel(DatasetMetadata dsm)
        {
            this.DatasetMetadataId = dsm.DatasetMetadataId;
            this.DatasetId = dsm.DatasetId;
            this.IsColumn = dsm.IsColumn;
            this.Name = dsm.Name;
            this.Value = dsm.Value;
        }

        public int DatasetMetadataId { get; set; }

        public int DatasetId { get; set; }

        public bool IsColumn { get; set; }

        public string Name { get; set; }

        public string Value { get; set;  }
    }
}