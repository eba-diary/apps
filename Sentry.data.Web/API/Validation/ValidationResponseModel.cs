using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class ValidationResponseModel : IResponseModel
    {
        public List<FieldValidationResponseModel> FieldValidations { get; set; }
    }
}