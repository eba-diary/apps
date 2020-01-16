using System.Net;
using System.Web.Http;
using System.Web.Http.Results;

namespace Sentry.data.Web.WebApi.Controllers
{
    public class BaseWebApiController : ApiController
    {


        protected StatusCodeResult NoContent()
        {
            return StatusCode(HttpStatusCode.NoContent);
        }


        //we could also implement a generice sharable apiResponse if needed/wanted
        //public ApiResponse SomeGeneraicResponse()
        //{
        //    return new ApiResponse
        //    {
        //        Success = true,
        //        ErrorMessage = { "", "" }.tolist()
        //    };
        //}

    }
}
