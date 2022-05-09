using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class DataInventorySearchResultRowModel
    {
        public string Asset { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string Object { get; set; }
        public string ObjectType { get; set; }
        public string Column { get; set; }
        public bool IsSensitive { get; set; }
        public string ProdType { get; set; }
        public string ColumnType { get; set; }
        public int MaxLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsNullable { get; set; }
        public string EffectiveDate { get; set; }
        public int BaseColumnId { get; set; }
        public bool IsOwnerVerified { get; set; }
        public List<string> AssetList { get; set; }
        public string SourceType { get; set; }
        public string ScanCategory { get; set; }
        public string ScanType { get; set; }
    }
}
