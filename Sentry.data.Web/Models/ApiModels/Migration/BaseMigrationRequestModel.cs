namespace Sentry.data.Web.Models.ApiModels.Migration
{
    public class BaseMigrationRequestModel
    {
        public int TargetDatasetId { get; set; }
        public string TargetDatasetNamedEnvironment { get; set; }
        public Core.GlobalEnums.NamedEnvironmentType TargetDatasetNamedEnvironmentType { get; set;}
    }
}