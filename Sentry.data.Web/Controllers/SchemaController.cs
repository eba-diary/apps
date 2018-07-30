using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
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
    public class SchemaController : ApiController
    {
        private MetadataRepositoryService _metadataRepositoryService;
        private IDatasetContext _dsContext;
        private IAssociateInfoProvider _associateInfoService;
        private UserService _userService;

        public SchemaController(MetadataRepositoryService metadataRepositoryService, IDatasetContext dsContext, IAssociateInfoProvider associateInfoService, UserService userService)
        {
            _metadataRepositoryService = metadataRepositoryService;
            _dsContext = dsContext;
            _associateInfoService = associateInfoService;
            _userService = userService;
        }

        public class Schema
        {

            public List<SchemaRow> rows { get; set; }
        }

        public class Metadata
        {
            public double DataLastUpdated { get; set; }
            public string Description { get; set; }
            public DropLocation DFSDropLocation { get; set; }
            public string DFSCronJob { get; set; }

            public DropLocation S3DropLocation { get; set; }
            public string S3CronJob { get; set; }

            public List<DropLocation> OtherJobs { get; set; }
            public List<string> CronJobs { get; set; }
        }

        public class DropLocation
        {
            public string Name { get; set; }
            public string Location { get; set; }
            public int JobId { get; set; }
        }
        

        public class SchemaRow
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
            public string Precision { get; set; }
            public string Scale { get; set; }

            public double LastUpdated { get; set; }
        }


        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetBasicMetadataInformationFor(int DatasetConfigID)
        {
            try
            {
                var config = _dsContext.getDatasetFileConfigs(DatasetConfigID);

                Metadata m = new Metadata();

                m.Description = config.Description;
                m.DFSDropLocation = config.RetrieverJobs.Where(x => x.DataSource.Is<DfsBasic>()).Select(x => new DropLocation() { Location = x.Schedule, Name = x.DataSource.SourceType, JobId = x.Id }).FirstOrDefault();

                if (config.DatasetFiles.Any())
                {
                    m.DataLastUpdated = config.DatasetFiles.Max(x => x.ModifiedDTM).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                }

                if (config.RetrieverJobs.Any(x => x.DataSource.Is<S3Basic>()))
                {
                    m.S3DropLocation = config.RetrieverJobs.Where(x => x.DataSource.Is<S3Basic>()).Select(x => new DropLocation() { Location = x.Schedule, Name = x.DataSource.SourceType, JobId = x.Id }).FirstOrDefault();
                }

               
                if (config.RetrieverJobs.Any(x => !x.DataSource.Is<S3Basic>() && !x.DataSource.Is<DfsBasic>()))
                {
                    m.OtherJobs = config.RetrieverJobs.Where(x => !x.DataSource.Is<S3Basic>() && !x.DataSource.Is<DfsBasic>()).OrderBy(x => x.Id)
                        .Select(x => new DropLocation() { Location = x.Schedule, Name = x.DataSource.SourceType, JobId = x.Id }).ToList();
                }

                return Ok(m);

            }
            catch
            {
                return NotFound();
            }
        }
    

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetColumnSchemaInformationFor(int DatasetConfigID)
        {
            try
            {
                var config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);
                var dataObject = _dsContext.DataObjects.Where(x => x.DataElement.DataElement_ID == config.DataElement_ID).FirstOrDefault();

                Schema s = new Schema() { rows = new List<SchemaRow>() };

                foreach (DataObjectField b in dataObject.DataObjectFields)
                {
                    SchemaRow r = new SchemaRow()
                    {
                        Name = b.DataObjectField_NME,
                        Description = b.DataObjectField_DSC,
                        LastUpdated = b.LastUpdt_DTM.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                        Type = b.DataObjectFieldDetails.FirstOrDefault(x => x.DataObjectFieldDetailType_CDE == "Datatype_TYP").DataObjectFieldDetailType_VAL
                    };

                    if(b.DataObjectFieldDetails.Any(x => x.DataObjectFieldDetailType_CDE == "Precision_AMT"))
                    {
                        r.Precision = b.DataObjectFieldDetails.FirstOrDefault(x => x.DataObjectFieldDetailType_CDE == "Precision_AMT") != null ?
                            b.DataObjectFieldDetails.FirstOrDefault(x => x.DataObjectFieldDetailType_CDE == "Precision_AMT").DataObjectFieldDetailType_VAL :
                            null;
                    }

                    if (b.DataObjectFieldDetails.Any(x => x.DataObjectFieldDetailType_CDE == "Scale_AMT"))
                    {
                        r.Scale = b.DataObjectFieldDetails.FirstOrDefault(x => x.DataObjectFieldDetailType_CDE == "Scale_AMT") != null ?
                            b.DataObjectFieldDetails.FirstOrDefault(x => x.DataObjectFieldDetailType_CDE == "Scale_AMT").DataObjectFieldDetailType_VAL :
                            null;
                    }

                    s.rows.Add(r);
                }

                

                return Ok(s);

            }
            catch
            {
                return NotFound();
            }
        }
    }
}
