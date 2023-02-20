using System;
using System.Linq.Expressions;

namespace Sentry.data.Web.API
{
    public class FluentValidationResponse
    {
        public ValidationResponseModel ValidationResponse { get; set; }
        public string PropertyValue { get; set; }
        public string PropertyName { get; set; }
    }

    public class FluentValidationResponse<TModel, TProperty> where TModel : IRequestModel
    {
        public ValidationResponseModel ValidationResponse { get; set; }
        public TProperty PropertyValue { get; set; }
        public string PropertyName { get; set; }
        public TModel RequestModel { get; set; }
    }
}