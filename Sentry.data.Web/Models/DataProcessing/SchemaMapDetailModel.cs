namespace Sentry.data.Web
{
    public class SchemaMapDetailModel : SchemaMapModel
    {
        public SchemaMapDetailModel(Core.SchemaMapDetailDto dto) : base(dto)
        {
            DatasetName = dto.DatasetName;
            SchemaName = dto.SchemaName;
            DatasetDetailUrl = $"{Configuration.Config.GetHostSetting("SentryDataBaseUrl")}/Dataset/Detail/{dto.DatasetId}";
        }
        public string DatasetName { get; set; }
        public string SchemaName { get; set; }
        public string DatasetDetailUrl { get; set; }
    }
}