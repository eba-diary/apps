using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sentry.data.Core
{
    public class ConfigService : IConfigService
    {
        public IDatasetContext _datasetContext;
        public IMessagePublisher _publisher;
        public IUserService _userService;
        public IEventService _eventService;
        public IMessagePublisher _messagePublisher;

        public ConfigService(IDatasetContext dsCtxt, IMessagePublisher publisher, 
            IUserService userService, IEventService eventService, IMessagePublisher messagePublisher)
        {
            _datasetContext = dsCtxt;
            _publisher = publisher;
            _userService = userService;
            _eventService = eventService;
            _messagePublisher = messagePublisher;
        }
        public SchemaDTO GetSchemaDTO(int id)
        {
            return MapToDto(_datasetContext.GetById<DataElement>(id));
        }

        public IList<ColumnDTO> GetColumnDTO(int id)
        {
            return MapToDto(_datasetContext.GetById<DataElement>(id).DataObjects);
        }

        
        public void UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows)
        {
            DatasetFileConfig config = _datasetContext.GetById<DatasetFileConfig>(configId);
            DataElement schema = _datasetContext.GetById<DataElement>(schemaId);
            DataElement newRevision = null;
            DataObject DOBJ = null;
            Boolean newRev = false;
            if (schema.DataObjects.Count == 0)
            {
                //This is a new configuration with no schema defined.
                DOBJ = new DataObject()
                {
                    DataObjectCreate_DTM = DateTime.Now,
                    DataObjectChange_DTM = DateTime.Now,
                    LastUpdt_DTM = DateTime.Now,
                    DataObject_NME = schema.SchemaName,
                    DataObject_DSC = schema.SchemaDescription,
                    DataObject_CDE = config.FileExtension.Id.ToString(),
                    DataObjectCode_DSC = config.FileExtension.Name
                };

                schema.DataObjects.Add(DOBJ);
                //_datasetContext.Merge(DOBJ);
                //DOBJ = _datasetContext.SaveChanges();
            }
            else
            //else if(schema.DataObjects.Count == 1 && schema.DataObjects[0].DataObjectFields.Count == 0)
            {
                //Add fields to Existing Data Object
                DOBJ = schema.DataObjects.Single();
            }
            //else
            //{
            //    newRev = true;
            //    DataElement maxRevision = config.Schema.OrderByDescending(o => o.SchemaRevision).Take(1).FirstOrDefault();
            //    newRevision = new DataElement()
            //    {
            //        DataElementCreate_DTM = DateTime.Now,
            //        DataElementChange_DTM = DateTime.Now,
            //        LastUpdt_DTM = DateTime.Now,
            //        DataElement_NME = maxRevision.DataElement_NME,
            //        DataElement_DSC = maxRevision.DataElement_DSC,
            //        DataElement_CDE = maxRevision.DataElement_CDE,
            //        DataElementCode_DSC = maxRevision.DataElementCode_DSC,
            //        DataElementDetails = maxRevision.DataElementDetails
            //    };

            //    //This is a new configuration with no schema defined.
            //    DOBJ = new DataObject()
            //    {
            //        DataObjectCreate_DTM = DateTime.Now,
            //        DataObjectChange_DTM = DateTime.Now,
            //        LastUpdt_DTM = DateTime.Now,
            //        DataObject_NME = schema.SchemaName,
            //        DataObject_DSC = schema.SchemaDescription,
            //        DataObject_CDE = config.FileExtension.Id.ToString(),
            //        DataObjectCode_DSC = config.FileExtension.Name
            //    };

            //    List<DataObject> dobjList = new List<DataObject>
            //    {
            //        DOBJ
            //    };
            //    newRevision.DataObjects = dobjList;

            //    newRevision.SchemaRevision += 1;
            //    newRevision.SchemaIsPrimary = false;

            //    DOBJ = newRevision.DataObjects.Single();

            //    //Schema revision(s) exist, therefore, Create new schema revision
            //    //  Retrieve max schema revision (data element)
            //    //  Use Data Element values to create new data element and increment schema revision
            //    //  Use Data Object values to create new data object for element created above
            //    //  add Data Object Field \ Detail values from incoming data
            //    //  
            //}

            List<DataObjectField> dofList = new List<DataObjectField>();

            foreach (SchemaRow sr in schemaRows)
            {
                DataObjectField dof = null;

                //Update Row
                if (sr.DataObjectField_ID != 0 && newRev == false)
                {
                    dof = DOBJ.DataObjectFields.Where(w => w.DataObjectField_ID == sr.DataObjectField_ID).FirstOrDefault();
                    dof.DataObjectField_NME = sr.Name;
                    dof.DataObjectField_DSC = sr.Description;
                    dof.LastUpdt_DTM = DateTime.Now;
                    dof.DataObjectFieldChange_DTM = DateTime.Now;
                }
                //Add New Row
                else
                {
                    dof = new DataObjectField();
                    dof.DataObject = DOBJ;
                    dof.DataObjectFieldCreate_DTM = DateTime.Now;
                    dof.DataObjectField_NME = sr.Name;
                    dof.DataObjectField_DSC = sr.Description;
                    dof.LastUpdt_DTM = DateTime.Now;
                    dof.DataObjectFieldChange_DTM = DateTime.Now;
                }


                if (sr.Type != "ARRAY")
                {
                    dof.DataType = sr.Type;
                }
                else
                {
                    dof.DataType = sr.Type + "<" + sr.ArrayType + ">";
                }

                if (sr.Nullable != null) { dof.Nullable = sr.Nullable ?? null; }
                if (sr.Precision != null)
                {
                    if (sr.Type == "DECIMAL")
                    {
                        dof.Precision = sr.Precision;
                    }
                    else
                    {
                        dof.Precision = null;
                    }
                }
                if (sr.Scale != null)
                {
                    if (sr.Type == "DECIMAL")
                    {
                        dof.Scale = sr.Scale;
                    }
                    else
                    {
                        dof.Scale = null;
                    }
                }

                dofList.Add(dof);


            }

            DOBJ.DataObjectFields = dofList;

            _datasetContext.Merge(schema);
            _datasetContext.SaveChanges();

            if (newRev == true)
            {
                _datasetContext.Merge(newRevision);
                _datasetContext.SaveChanges();
            }

            ////Write message to create associated hive table
            //dynamic msg1 = new JObject();
            //string eventTopic = $"{Configuration.Config.GetSetting("SAIDKey").ToUpper()}-{Configuration.Config.GetHostSetting("EnvironmentName").ToUpper()}-{Configuration.Config.GetHostSetting("DSCEventTopic").ToUpper()}";
            ////Write file information to topic
            //try
            //{
            //    msg1.EventType = "HIVE-TABLE-CREATE";
            //    msg1.Schema = new JObject();
            //    msg1.Schema.SchemaId = schema.DataElement_ID;
            //    msg1.Schema.Format = schema.FileFormat;
            //    msg1.Schema.Header = "true";
            //    msg1.Schema.Delimiter = schema.Delimiter;
            //    msg1.Schema.HiveDatabase = schema.HiveDatabase;
            //    msg1.Schema.HiveTable = schema.HiveTable;
            //    msg1.Schema.Columns = new JObject();

            //    msg1.SourceBucket = Configuration.Config.GetHostSetting("AWSRootBucket");
            //    msg1.SourceKey = df_newParent.FileLocation;
            //    msg1.SourceVersionId = df_newParent.VersionId;

            //    _messagePublisher.Publish(eventTopic, df_newParent.Schema.DataElement_ID.ToString(), msg1.ToString());
            //}
            //catch (Exception ex)
            //{
            //    job.JobLoggerMessage("ERROR", $"Failed writing SCHEMA-RAWFILE-ADD event - key:{schema.DataElement_ID.ToString()} | topic:{eventTopic} | message:{msg1.ToString()})", ex);
            //}

            HiveTableCreateModel hiveCreate = new HiveTableCreateModel();

            SchemaModel sm = new SchemaModel();
            sm.SchemaID = schema.DataElement_ID;
            sm.Format = schema.FileFormat;
            sm.Header = "true";
            sm.Delimiter = schema.Delimiter;
            sm.HiveDatabase = schema.HiveDatabase;
            sm.HiveTable = schema.HiveTable;

            DataObject dObj = schema.DataObjects.FirstOrDefault();

            List<ColumnModel> ColumnModelList = new List<ColumnModel>();

            if (dObj != null)
            {
                foreach (DataObjectField dof in dObj.DataObjectFields)
                {
                    ColumnModel cm = new ColumnModel();
                    cm.Name = dof.DataObjectField_NME;
                    cm.DataType = dof.DataType;
                    cm.Nullable = dof.Nullable.ToString();
                    cm.Length = dof.Length;
                    cm.Precision = dof.Precision;
                    cm.Scale = dof.Scale;

                    ColumnModelList.Add(cm);
                }
            }

            sm.Columns = ColumnModelList;

            hiveCreate.Schema = sm;

            _messagePublisher.PublishDSCEvent(schema.DataElement_ID.ToString(), JsonConvert.SerializeObject(hiveCreate));

            //UpdateHiveTableStatus(schema, HiveTableStatusEnum.Requested);           
            
        }

        //private void UpdateHiveTableStatus(DataElement schema, HiveTableStatusEnum requested)
        //{
        //    schema.HiveTableStatus = requested.ToString();

        //    _datasetContext.Merge(schema);
        //    _datasetContext.SaveChanges();
        //}

        private SchemaDTO MapToDto(DataElement dataElement)
        {
            SchemaDTO dto = new SchemaDTO()
            {
                SchemaID = dataElement.DataElement_ID,
                Format = dataElement.FileFormat,
                Delimiter = dataElement.Delimiter,
                Header = dataElement.HasHeader,
                HiveDatabase = dataElement.HiveDatabase,
                HiveTable = dataElement.HiveTable,
                HiveStatus = dataElement.HiveTableStatus
            };

            return dto;
        }
        private IList<ColumnDTO> MapToDto(IList<DataObject> objects)
        {
            IList<ColumnDTO> dtoList = new List<ColumnDTO>();
            foreach (DataObject table in objects)
            {
                foreach (DataObjectField field in table.DataObjectFields)
                {
                    ColumnDTO dto = new ColumnDTO()
                    {
                        Name = field.DataObjectField_NME,
                        DataType = field.DataType,
                        Length = field.Length,
                        Nullable = field.Nullable,
                        Precision = field.Precision,
                        Scale = field.Scale
                    };
                    dtoList.Add(dto);
                }
            }
            return dtoList;
        }
    }
}
