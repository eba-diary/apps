using System;

namespace Sentry.data.Core
{
    public class SchemaDTO
    {
        public int SchemaID { get; set; }
        public string Format { get; set; }
        public string Header { get; set; }
        public string Delimiter { get; set; }
        public string HiveTable { get; set; }
        public string HiveDatabase { get; set; }
        public string HiveStatus { get; set; }
        public string HiveLocation { get; set; }
    }
}