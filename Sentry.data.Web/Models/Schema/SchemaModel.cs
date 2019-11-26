using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class SchemaModel
    {
        public SchemaModel(SchemaApiDTO dto)
        {
            SchemaID = dto.SchemaID;
            Format = dto.Format;
            Header = dto.Header;
            Delimiter = dto.Delimiter;
            HiveTable = dto.HiveTable;
            HiveDatabase = dto.HiveDatabase;
            HiveTableStatus = dto.HiveStatus;
            HiveLocation = dto.HiveLocation;
            CurrentView = dto.CurrentView;
        }

        public int SchemaID { get; set; }
        public string Format { get; set; }
        public string Header { get; set; }
        public string Delimiter { get; set; }
        public string HiveTable { get; set; }
        public string HiveDatabase { get; set; }
        public string HiveTableStatus { get; set; }
        public string HiveLocation { get; set; }
        public bool CurrentView { get; set; }
    }
}