using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.Controllers
{
    public class TagController : ApiController
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


        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
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

            return Ok(reply.Distinct().OrderBy(x => x.Name).ThenByDescending(x => x.Count));
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> CheckNewTagName(string name)
        {
            Boolean reply = _dsContext.Tags.Count(x => x.Name.ToLower() == name.ToLower()) == 0 ? true : false;

            return Ok(reply);
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
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
