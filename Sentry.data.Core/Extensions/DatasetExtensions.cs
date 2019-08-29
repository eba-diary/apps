namespace Sentry.data.Core
{
    public static class DatasetExtensions
    {
        public static string GenerateSASLibary(this DatasetDto dto, IDatasetContext dsContext)
        {
            return CommonExtensions.GenerateSASLibaryName(dsContext.GetById<Dataset>(dto.DatasetId));
        }
    }
}
