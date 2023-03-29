namespace Sentry.data.Web.API
{
    public class FluentValidationResponse<TModel, TProperty> where TModel : IRequestModel
    {
        public ConcurrentValidationResponse ValidationResponse { get; set; }
        public TProperty PropertyValue { get; set; }
        public string PropertyName { get; set; }
        public bool IsRequiredProperty { get; set; }
        public TModel RequestModel { get; set; }
    }
}