using Newtonsoft.Json.Linq;
using Sentry.data.Core.Entities.Metadata;
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
            DataElement de = _datasetContext.GetById<DataElement>(id);
            SchemaDTO dto = new SchemaDTO();
            MapToDto(de, dto);
            return dto;
        }

        public SchemaDetailDTO GetSchemaDetailDTO(int id)
        {
            DataElement de = _datasetContext.GetById<DataElement>(id);
            SchemaDetailDTO dto = new SchemaDetailDTO();
            MaptToDetailDto(de, dto);
            return dto;
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

            HiveTableCreateModel hiveCreate = new HiveTableCreateModel();
            schema.ToHiveCreateModel(hiveCreate);

            _messagePublisher.PublishDSCEvent(schema.DataElement_ID.ToString(), JsonConvert.SerializeObject(hiveCreate));       
            
        }

        private void MapToDto(DataElement de, SchemaDTO dto)
        {
            dto.SchemaID = de.DataElement_ID;
            dto.Format = de.FileFormat;
            dto.Delimiter = de.Delimiter;
            dto.Header = de.HasHeader.ToString();
            dto.HiveDatabase = de.HiveDatabase;
            dto.HiveTable = de.HiveTable;
            dto.HiveStatus = de.HiveTableStatus;
            dto.HiveLocation = de.HiveLocation;
        }

        private void MaptToDetailDto(DataElement de, SchemaDetailDTO dto)
        {
            MapToDto(de, dto);

            List<SchemaRow> rows = new List<SchemaRow>();
            DataObject dobj = de.DataObjects.FirstOrDefault();
            IList<DataObjectField> dofs = (dobj != null) ? dobj.DataObjectFields : null;

            if (dofs != null)
            {
                foreach (DataObjectField b in dofs)
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
                    if (b.Nullable != null) { r.Nullable = b.Nullable ?? null; }
                    if (b.Length != null) { r.Length = b.Length ?? null; }
                    rows.Add(r);
                }
            }            

            dto.Rows = rows;
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
