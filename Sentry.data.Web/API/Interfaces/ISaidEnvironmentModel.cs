namespace Sentry.data.Web.API
{
    public interface ISaidEnvironmentModel
    {
        string SaidAssetCode { get; set; }
        string NamedEnvironment { get; set; }
        string NamedEnvironmentTypeCode { get; set; }
    }
}
