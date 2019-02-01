namespace Sentry.data.Core
{
    public class ColumnDTO
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool? Nullable { get; set; }
        public string Length { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }
    }
}