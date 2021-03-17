using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using System.Web;

namespace Sentry.data.Web
{
    /// <summary>
    /// Global WebAPI Unhandled Exception Handler
    /// </summary>
    public class UnhandledExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            var rootException = context.Exception.GetBaseException();

            if ((rootException) is NotAuthorizedException)
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                    // globally handle NotAuthorizedException as 403 Forbidden
                    context.Result = new ResponseMessageResult(context.Request.CreateResponse(System.Net.HttpStatusCode.Forbidden, rootException.Message));
                else
                    context.Result = new ResponseMessageResult(context.Request.CreateResponse(System.Net.HttpStatusCode.Unauthorized, rootException.Message));
            }
            else
                context.Result = new ResponseMessageResult(context.Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, rootException.Message));
        }
    }
}