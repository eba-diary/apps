﻿using Sentry.data.Common;
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

        public class OutputSchema
        {
            public List<SchemaRow> rows { get; set; }
            public int RowCount { get; set; }
            public string HiveTableName { get; set; }
            public string HiveDatabaseName { get; set; }
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

         //   public int Views { get; set; }
         //   public int Downloads { get; set; }
        }

        public class DropLocation
        {
            public string Name { get; set; }
            public string Location { get; set; }
            public int JobId { get; set; }
            public Boolean IsEnabled { get; set; }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetBasicMetadataInformationFor(int DatasetConfigID)
        {
            try
            {
                DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);
                //DatasetFileConfig config = _dsContext.getDatasetFileConfigs(DatasetConfigID);

                Event e = new Event();
                e.EventType = _dsContext.EventTypes.Where(w => w.Description == "Viewed").FirstOrDefault();
                e.Status = _dsContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                e.TimeCreated = DateTime.Now;
                e.TimeNotified = DateTime.Now;
                e.IsProcessed = false;
                e.UserWhoStartedEvent = RequestContext.Principal.Identity.Name;
                e.DataConfig = config.ConfigId;
                e.Reason = "Viewed Schema for Dataset";
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                Metadata m = new Metadata();

                m.Description = config.Description;
                //m.DFSDropLocation = config.RetrieverJobs.Where(x => x.DataSource.Is<DfsBasic>()).Select(x => new DropLocation() { Location = x.Schedule, Name = x.DataSource.SourceType, JobId = x.Id }).FirstOrDefault();

                //m.Views = _dsContext.Events.Where(x => x.Reason == "Viewed Schema for Dataset" && x.DataConfig == DatasetConfigID).Count();
                //m.Downloads = _dsContext.Events.Where(x => x.EventType.Description == "Downloaded Data File" && x.DataConfig == DatasetConfigID).Count();

                if (config.DatasetFiles.Any())
                {
                    m.DataLastUpdated = config.DatasetFiles.Max(x => x.ModifiedDTM).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                }

                if (config.RetrieverJobs.Any(x => x.DataSource.Is<S3Basic>()))
                {
                   // m.S3DropLocation = config.RetrieverJobs.Where(x => x.DataSource.Is<S3Basic>()).Select(x => new DropLocation() { Location = x.Schedule, Name = x.DataSource.SourceType, JobId = x.Id }).FirstOrDefault();
                }

               
                if (config.RetrieverJobs.Any(x => !x.DataSource.Is<S3Basic>() && !x.DataSource.Is<DfsBasic>()))
                {
                    m.OtherJobs = config.RetrieverJobs.Where(x => !x.DataSource.Is<S3Basic>() && !x.DataSource.Is<DfsBasic>()).OrderBy(x => x.Id)
                        .Select(x => new DropLocation() { Location = x.IsEnabled ? x.Schedule : "Disabled", Name = x.DataSource.SourceType, JobId = x.Id, IsEnabled = x.IsEnabled }).ToList();
                }


                return Ok(m);

            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetPrimaryHiveTableFor(int DatasetConfigID, int SchemaID = 0)
        {
            DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

            if (config.Schema.Any(x => x.SchemaIsPrimary))
            {
                DataElement schemarev = config.Schema.FirstOrDefault(x => x.SchemaIsPrimary);

                if (schemarev.HiveTable != null)
                {
                    return Ok(new { HiveDatabaseName = schemarev.HiveDatabase, HiveTableName = schemarev.HiveTable });
                }
                else
                {
                    return NotFound();
                }
                
            }
            else {
                return NotFound();
            }
        }





        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetColumnSchemaInformationFor(int DatasetConfigID, int SchemaID = 0)
        {
            try
            {
                DatasetFileConfig config = _dsContext.GetById<DatasetFileConfig>(DatasetConfigID);

                if (config.Schema.Any())
                {
                    var a = config.Schema.ToList();

                    DataElement schema = null;

                    if (SchemaID != 0)
                    {
                        schema = config.Schema.Where(x => x.DataElement_ID == SchemaID).FirstOrDefault();
                    }
                    else
                    {
                        if(config.Schema.Any(x => x.SchemaIsPrimary))
                        {
                            schema = config.Schema.Where(x => x.SchemaIsPrimary).OrderBy(x => x.SchemaRevision).FirstOrDefault();
                        }
                        else
                        {
                            schema = config.Schema.OrderBy(x => x.SchemaRevision).FirstOrDefault();
                        }
                    }

                    if (schema.DataObjects.Count > 0)
                    {
                        OutputSchema s = new OutputSchema();

                        s.rows = new List<SchemaRow>();

                        if (schema.DataObjects.Any(x => x.RowCount != 0))
                        {
                            s.RowCount = Convert.ToInt32(schema.DataObjects.FirstOrDefault().RowCount);
                        }

                        foreach (DataObjectField b in schema.DataObjects.FirstOrDefault().DataObjectFields)
                        {
                            SchemaRow r = new SchemaRow()
                            {
                                Name = b.DataObjectField_NME,
                                DataObjectField_ID = b.DataObjectField_ID,
                                Description = b.DataObjectField_DSC,
                                LastUpdated = b.LastUpdt_DTM.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
                            };

                            r.Type = (!String.IsNullOrEmpty(b.DataType)) ? b.DataType.ToUpper() : "VARCHAR";
                            if (b.Precision != null) { r.Precision = b.Precision ?? null; }
                            if (b.Scale != null) { r.Scale = b.Scale ?? null; }
                            //r.Precision = (b.Precision != null && !String.IsNullOrEmpty(b.Precision)) ? b.Precision : null;
                            //r.Scale = (b.Scale != null && !String.IsNullOrEmpty(b.Scale)) ? b.Scale : null;
                            if (b.Nullable != null) { r.Nullable = b.Nullable ?? null; }
                            if (b.Length != null) { r.Length = b.Length ?? null; }
                            s.rows.Add(r);
                        }

                        return Ok(s);
                    }
                    else
                    {
                        OutputSchema s = new OutputSchema();
                        s.rows = new List<SchemaRow>();
                        return Ok(s);
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch(Exception ex)
            {
                return NotFound();
            }
        }







    }
}
