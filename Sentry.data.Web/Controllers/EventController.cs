using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.Controllers
{
    public class EventController : ApiController
    {
        public IDatasetContext _datasetContext;

        public EventController(IDatasetContext dsCtxt)
        {
            _datasetContext = dsCtxt;
        }

        [HttpPost]
        [Route("Create")]
        public HttpResponseMessage Create(Event e)
        {
            _datasetContext.Merge<Event>(e);
            _datasetContext.SaveChanges();

            return new HttpResponseMessage(HttpStatusCode.Created);
        }

    }
}