namespace Sentry.data.Core
{
    public class DatasetFileParquet
    {
        public virtual int DatsetFileParquetId { get; set; }
        public virtual int DatasetFileId { get; set; }
        public virtual int SchemaId { get; set; }
        public virtual string FileLocation { get; set; }
        public virtual int DatasetId { get; set; }
    }
}
