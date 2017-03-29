using System;
using System.Net;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Sentry.data.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class ErrorController : Controller
    {
        public ErrorController() { }

        [Route("ErrorTester/{errorNumber:int?}")]
        public ActionResult Index(int errorNumber = 0)
        {
            switch (errorNumber)
            {
                case 401:
                    return new HttpUnauthorizedResult();
                case 403:
                    throw new NotAuthorizedException(null);
                case 500:
                    throw new Exception();
                default:
                    return View();
            }
        }

        [Route("Unauthorized")]
        [AllowUnAuthorized()]
        public ActionResult Unauthorized()
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.TrySkipIisCustomErrors = true;

            return View();
        }

        [Route("NotFound")]
        [AllowUnAuthorized()]
        public ActionResult NotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            Response.TrySkipIisCustomErrors = true;

            return View();
        }

        [Route("ServerError")]
        [AllowUnAuthorized()]
        public ActionResult ServerError()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Response.TrySkipIisCustomErrors = true;

            return View();
        }

        [Route("Forbidden")]
        [AllowUnAuthorized()]
        public ActionResult Forbidden()
        {
            Response.StatusCode = (int)HttpStatusCode.Forbidden;
            Response.TrySkipIisCustomErrors = true;

            return View();
        }
    }
}
