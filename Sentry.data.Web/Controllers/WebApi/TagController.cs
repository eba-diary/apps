using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_TAG)]
    [AuthorizeByPermission(PermissionNames.QueryToolUser)]
    public class TagController : BaseWebApiController
    {
        private MetadataRepositoryService _metadataRepositoryService;
        private IDatasetContext _dsContext;
        private IAssociateInfoProvider _associateInfoService;
        private UserService _userService;

        public TagController(MetadataRepositoryService metadataRepositoryService, IDatasetContext dsContext, IAssociateInfoProvider associateInfoService, UserService userService)
        {
            _metadataRepositoryService = metadataRepositoryService;
            _dsContext = dsContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
        }

        /// <summary>
        /// search tags based on query string
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SearchableTag>))]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Type = typeof(Exception))]
        public async Task<IHttpActionResult> SearchTags(string query = null)
        {
            List<SearchableTag> reply = new List<SearchableTag>();

            try
            {
                if (query != null)
                {
                    var tempReply = _dsContext.Tags.Where(x => x.Name.ToLower().Contains(query.ToLower()));

                    foreach (var temp in tempReply)
                    {
                        reply.Add(temp.GetSearchableTag());
                    }
                }
                else
                {
                    var tempReply = _dsContext.Tags;

                    foreach (var temp in tempReply)
                    {
                        reply.Add(temp.GetSearchableTag());
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(reply.Distinct().OrderBy(x => x.Name).OrderByDescending(x => x.Count));
        }

        /// <summary>
        /// check if the tag name already exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{name}")]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SearchableTag>))]
        public async Task<IHttpActionResult> CheckNewTagName(string name)
        {
            Boolean reply =  _dsContext.Tags.Count(x => x.Name.ToLower() == name.ToLower()) == 0 ? true : false;

            return Ok(reply);
        }

        /// <summary>
        /// create a new tag
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{name}/{description}")]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SearchableTag>))]
        public async Task<IHttpActionResult> AddNewTag(string name, string description)
        {
            Boolean reply = _dsContext.Tags.Count(x => x.Name.ToLower() == name.ToLower()) == 0 ? true : false;

            if (reply == false)
            {
                return BadRequest("A tag with this name already exists.");
            }

            _dsContext.Add<MetadataTag>(
                new MetadataTag() {
                    Name = name,
                    Description = description,
                    CreatedBy = RequestContext.Principal.Identity.Name,
                    Created = DateTime.Now,
                    Group = _dsContext.TagGroups.Where(w => w.Name == "Other").First()
                });

            _dsContext.SaveChanges();

            return Ok();
        }



    }
}
