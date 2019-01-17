using Sentry.Core;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web.Controllers
{
    public class BaseController : Controller
    {

        public SharedContextModel SharedContext { get; set; }

        /// <summary>
        /// After catching a Sentry.Core.ValidationException in a controller action,
        /// pass it to this function to add the individual errors to the model
        /// so that they correctly display on the form.  Override this method in your
        /// controller to map specific core validation exceptions to specific fields
        /// on your form.
        /// </summary>
        /// <param name="ex">The Sentry.Core.ValidationException</param>
        protected virtual void AddCoreValidationExceptionsToModel(ValidationException ex)
        {
            foreach (ValidationResult vr in ex.ValidationResults.GetAll())
            {
                ModelState.AddModelError(string.Empty, vr.Description);
            }
        }

        protected virtual void AddCoreValidationExceptionsToModel(List<string> errors)
        {
            errors.ForEach(x => ModelState.AddModelError(string.Empty, x));
        }

        protected JsonResult AjaxSuccessJson()
        {
            return Json(new {Success = true});
        }
    }
}
