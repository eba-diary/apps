using System;
using System.Linq.Expressions;

namespace Sentry.data.Web.API
{
    public class FluentValidationResponse
    {
        public ValidationResponseModel ValidationResponse { get; set; }
        public Expression<Func<string>> PropertyValueExpression { get; set; }
    }
}