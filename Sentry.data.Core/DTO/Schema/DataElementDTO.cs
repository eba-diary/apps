using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataElementDto
    {
        public string DataElementName { get; set; }
        public string SchemaName { get; set; }
        public string SchemaDescription { get; set; }
        public bool SchemaIsForceMatch { get; set; }
        public bool SchemaIsPrimary { get; set; }
        public string Delimiter { get; set; }
        public DateTime DataElementChange_DTM { get; set; }
        public bool HasHeader { get; set; }
        public int FileFormatId { get; set; }
        public bool CreateCurrentView { get; set; }
        public int ParentDatasetId { get; set; }
        public int FileExtensionId { get; set; }
        public int DataElementID { get; set; }
        public string SasLibrary { get; set; }
        public string ControlMTriggerName { get; set; }
    }
}
