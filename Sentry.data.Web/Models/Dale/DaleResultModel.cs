using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class DaleResultModel
    {

        public string Server { get; set; }
        public string Database { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
        public string ColumnType { get; set; }
        public int PrecisionLength { get; set; }
        public int ScaleLength { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime LastScanDate { get; set; }



    }
}
