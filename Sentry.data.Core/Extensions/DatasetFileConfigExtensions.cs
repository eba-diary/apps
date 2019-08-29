namespace Sentry.data.Core
{
    public static class DatasetFileConfigExtensions
    {
        public static string GenerateSASLibaryName(this DatasetFileConfigDto dto, IDatasetContext dsContext)
        {
            return CommonExtensions.GenerateSASLibaryName(dsContext.GetById<Dataset>(dto.ParentDatasetId));
        }
    }
}
