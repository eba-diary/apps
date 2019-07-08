using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sentry.Common.Logging;

namespace Sentry.data.Core
{
    public class ConfigService : IConfigService
    {
        public IDatasetContext _datasetContext;
        public IMessagePublisher _publisher;
        public IUserService _userService;
        public IEventService _eventService;
        public IMessagePublisher _messagePublisher;
        public IEncryptionService _encryptService;

        public ConfigService(IDatasetContext dsCtxt, IMessagePublisher publisher, 
            IUserService userService, IEventService eventService, IMessagePublisher messagePublisher,
            IEncryptionService encryptService)
        {
            _datasetContext = dsCtxt;
            _publisher = publisher;
            _userService = userService;
            _eventService = eventService;
            _messagePublisher = messagePublisher;
            _encryptService = encryptService;
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

        public List<string> Validate(DataElementDto dto)
        {
            List<string> errors = new List<string>();

            var currentFileExtension = _datasetContext.FileExtensions.FirstOrDefault(x => x.Id == dto.FileFormatId).Name.ToLower();

            if (currentFileExtension == "csv" && dto.Delimiter != ",")
            {
                errors.Add("File Extension CSV and it's delimiter do not match.");
            }

            if (currentFileExtension == "delimited" && string.IsNullOrWhiteSpace(dto.Delimiter))
            {
                errors.Add("File Extension Delimited is missing it's delimiter.");
            }

            return errors;
        }

        public List<string> Validate(DataSourceDto dto)
        {
            List<string> errors = new List<string>();

            AuthenticationType auth = _datasetContext.GetById<AuthenticationType>(Convert.ToInt32(dto.AuthID));

            switch (dto.SourceType)
            {
                case "DFSCustom":
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is DfsCustom && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An DFS Custom Data Source is already exists with this name.");
                    }
                    break;
                case "FTP":
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is FtpSource && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An FTP Data Source is already exists with this name.");
                    }
                    if (!(dto.BaseUri.ToString().StartsWith("ftp://")))
                    {
                        errors.Add("A valid FTP URI starts with ftp:// (i.e. ftp://foo.bar.com/base/dir)");
                    }
                    break;
                case "SFTP":
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is SFtpSource && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An SFTP Data Source is already exists with this name.");
                    }
                    if (!(dto.BaseUri.ToString().StartsWith("sftp://")))
                    {
                        errors.Add("A valid SFTP URI starts with sftp:// (i.e. sftp://foo.bar.com//base/dir/)");
                    }
                    break;
                case "HTTPS":
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is HTTPSSource && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An HTTPS Data Source is already exists with this name.");
                    }
                    if (!(dto.BaseUri.ToString().StartsWith("https://")))
                    {
                        errors.Add("A valid HTTPS URI starts with https:// (i.e. https://foo.bar.com/base/api/)");
                    }

                    //if token authentication, user must enter values for token header and value
                    if (auth.Is<TokenAuthentication>())
                    {
                        if (String.IsNullOrWhiteSpace(dto.TokenAuthHeader))
                        {
                            errors.Add("Token Authenticaion requires a token header");
                        }

                        if (String.IsNullOrWhiteSpace(dto.TokenAuthValue))
                        {
                            errors.Add("Token Authentication requires a token header value");
                        }
                    }

                    foreach (RequestHeader h in dto.RequestHeaders)
                    {
                        if (String.IsNullOrWhiteSpace(h.Key) || String.IsNullOrWhiteSpace(h.Value))
                        {
                            errors.Add("Request headers need to contain valid values");
                        }
                    }
                    break;
                case "GOOGLEAPI":
                    if (!(dto.BaseUri.ToString().StartsWith("https://")))
                    {
                        errors.Add("A valid GoogleApi URI starts with https:// (i.e. https://analyticsreporting.googleapis.com/)");
                    }
                    if (!(dto.BaseUri.ToString().Contains("googleapis.com")))
                    {
                        errors.Add("An invalid GoogleApi URI");
                    }
                    break;
                case "DFSBasic":
                default:
                    throw new NotImplementedException();
            }

            return errors;
        }

        public bool CreateAndSaveNewDataSource(DataSourceDto dto)
        {
            try
            {
                DataSource ds = CreateDataSource(dto);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating data source", ex);
                return false;
            }           

            return true;
        }

        public bool UpdateAndSaveDataSource(DataSourceDto dto)
        {
            try
            {
                DataSource dsrc = _datasetContext.GetById<DataSource>(dto.OriginatingId);
                UpdateDataSource(dto, dsrc);
                _datasetContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving data source", ex);
                return false;
            }
        }

        private void UpdateDataSource(DataSourceDto dto, DataSource dsrc)
        {
            MapDataSource(dto, dsrc);
        }

        private void MapDataSource(DataSourceDto dto, DataSource dsrc)
        {
            dsrc.Name = dto.Name;
            dsrc.Description = dto.Description;
            dsrc.SourceType = dto.SourceType;
            dsrc.SourceAuthType = _datasetContext.GetById<AuthenticationType>(int.Parse(dto.AuthID));
            dsrc.IsUserPassRequired = dto.IsUserPassRequired;
            dsrc.PortNumber = dto.PortNumber;
            dsrc.BaseUri = dto.BaseUri;

            MapDataSourceSpecific(dto, dsrc);

        }

        private void MapDataSourceSpecific(DataSourceDto dto, DataSource dsrc)
        {
            if (dsrc.Is<HTTPSSource>())
            {
                ((HTTPSSource)dsrc).ClientId = dto.ClientId;
                ((HTTPSSource)dsrc).TokenUrl = dto.TokenUrl;
                ((HTTPSSource)dsrc).TokenExp = dto.TokenExp;
                ((HTTPSSource)dsrc).AuthenticationHeaderName = dto.TokenAuthHeader;
                ((HTTPSSource)dsrc).RequestHeaders = (dto.RequestHeaders.Any()) ? dto.RequestHeaders : null;

                UpdateClaims((HTTPSSource)dsrc, dto);
            }

            // only update if new value is supplied
            if (dto.TokenAuthValue != null)
            {
                ((HTTPSSource)dsrc).AuthenticationTokenValue = _encryptService.EncryptString(dto.TokenAuthValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), dto.IVKey).Item1;
            }

            // only update if new value is supplied
            if (dto.ClientPrivateId != null)
            {
                ((HTTPSSource)dsrc).ClientPrivateId = _encryptService.EncryptString(dto.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), dto.IVKey).Item1;
            }
                
        }

        private void UpdateClaims(HTTPSSource dsrc, DataSourceDto dto)
        {
            //foreach claim type in dsrc
            // does claim type exist in dto?
            //if yes, update if value is different
            //if no, create OAuthClaim with dto value

            foreach (OAuthClaim item in dsrc.Claims)
            {
                switch (item.Type)
                {
                    case GlobalEnums.OAuthClaims.iss:
                        if (dto.ClientId != null && dto.ClientId != item.Value) { item.Value = dto.ClientId; }
                        break;
                    case GlobalEnums.OAuthClaims.aud:
                        if (dto.TokenUrl != null && dto.TokenUrl != item.Value) { item.Value = dto.TokenUrl; }
                        break;
                    case GlobalEnums.OAuthClaims.exp:
                        if (dto.TokenExp != 0 && dto.TokenExp.ToString() != item.Value) { item.Value = dto.TokenExp.ToString(); }
                        break;
                    case GlobalEnums.OAuthClaims.scope:
                        if (dto.Scope != null && dto.Scope != item.Value) { item.Value = dto.Scope; }
                        break;
                    default:
                        break;
                }
            }


            ////Property exists on original source
            //if(dsrc.Claims.Any(w => w.Type == GlobalEnums.OAuthClaims.iss))
            //{
            //    //Property has changed
            //    if(dsrc.Claims.Where(w => w.Type == GlobalEnums.OAuthClaims.iss).Select(s => s.Value).SingleOrDefault() != dto.ClientId)
            //    {
            //        //Update property
            //        dsrc.ClientId = dto.ClientId;
            //    }
            //}
            ////Property does not exist on original source
            //else
            //{
            //    dsrc.ClientId = dto.ClientId;
            //}
        }

        private DataSource CreateDataSource(DataSourceDto dto)
        {
            DataSource source = null;

            AuthenticationType auth = _datasetContext.GetById<AuthenticationType>(Convert.ToInt32(dto.AuthID));

            switch (dto.SourceType)
            {
                case "DFSBasic":
                    source = new DfsBasic();
                    break;
                case "DFSCustom":
                    source = new DfsCustom();
                    break;
                case "FTP":
                    source = new FtpSource();
                    break;
                case "SFTP":
                    source = new SFtpSource();
                    break;
                case "HTTPS":
                    source = new HTTPSSource();
                    ((HTTPSSource)source).IVKey = _encryptService.GenerateNewIV();

                    //Process only if headers exist
                    if (dto.RequestHeaders.Any())
                    {
                        ((HTTPSSource)source).RequestHeaders = dto.RequestHeaders;
                    }

                    if (auth.Is<TokenAuthentication>())
                    {
                        ((HTTPSSource)source).AuthenticationHeaderName = dto.TokenAuthHeader;
                        ((HTTPSSource)source).AuthenticationTokenValue = _encryptService.EncryptString(dto.TokenAuthValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)source).IVKey).Item1;
                    }

                    if (auth.Is<OAuthAuthentication>())
                    {
                        ((HTTPSSource)source).ClientId = dto.ClientId;
                        ((HTTPSSource)source).ClientPrivateId = _encryptService.EncryptString(dto.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)source).IVKey).Item1;
                        ((HTTPSSource)source).TokenUrl = dto.TokenUrl;
                        ((HTTPSSource)source).TokenExp = dto.TokenExp;
                        ((HTTPSSource)source).Scope = dto.Scope;
                    }
                    break;
                case "GOOGLEAPI":
                    source = new GoogleApiSource();
                    ((GoogleApiSource)source).IVKey = _encryptService.GenerateNewIV();

                    //Process only if headers exist
                    if (dto.RequestHeaders.Any())
                    {
                        ((GoogleApiSource)source).RequestHeaders = dto.RequestHeaders;
                    }

                    if (auth.Is<TokenAuthentication>())
                    {
                        ((GoogleApiSource)source).AuthenticationHeaderName = dto.TokenAuthHeader;
                        ((GoogleApiSource)source).AuthenticationTokenValue = _encryptService.EncryptString(dto.TokenAuthValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)source).IVKey).Item1;
                    }

                    if (auth.Is<OAuthAuthentication>())
                    {
                        ((GoogleApiSource)source).ClientId = dto.ClientId;
                        ((GoogleApiSource)source).ClientPrivateId = _encryptService.EncryptString(dto.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)source).IVKey).Item1;
                        ((GoogleApiSource)source).TokenUrl = dto.TokenUrl;
                        ((GoogleApiSource)source).TokenExp = dto.TokenExp;
                        ((GoogleApiSource)source).Scope = dto.Scope;
                    }
                    break;
                default:
                    throw new NotImplementedException("SourceType is not configured for save");
            }

            source.Name = dto.Name;
            source.Description = dto.Description;
            source.SourceAuthType = auth;
            source.IsUserPassRequired = dto.IsUserPassRequired;
            source.BaseUri = dto.BaseUri;
            source.PortNumber = dto.PortNumber;

            _datasetContext.Add(source);

            if (source.Is<HTTPSSource>() && auth.Is<OAuthAuthentication>())
            {
                CreateClaims(dto, (HTTPSSource)source);
            }

            return source;
        }

        private void CreateClaims(DataSourceDto dto, HTTPSSource source)
        {
            List<OAuthClaim> claimsList = new List<OAuthClaim>();
            source.Claims = claimsList;
            OAuthClaim claim;

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.iss, Value = dto.ClientId };
            _datasetContext.Add(claim);
            claimsList.Add(claim);

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.scope, Value = dto.Scope };
            _datasetContext.Add(claim);
            claimsList.Add(claim);

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.aud, Value = dto.TokenUrl };
            _datasetContext.Add(claim);
            claimsList.Add(claim);

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.exp, Value = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromMinutes(dto.TokenExp)).TotalSeconds.ToString() };
            _datasetContext.Add(claim);
            claimsList.Add(claim);
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


                if (sr.DataType != "ARRAY")
                {
                    dof.DataType = sr.DataType;
                }
                else
                {
                    dof.DataType = sr.DataType + "<" + sr.ArrayType + ">";
                }

                if (sr.Nullable != null) { dof.Nullable = sr.Nullable ?? null; }
                if (sr.Precision != null)
                {
                    if (sr.DataType == "DECIMAL")
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
                    if (sr.DataType == "DECIMAL")
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

        public bool UpdateandSaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime)
        {
            try
            {
                HTTPSSource updatedSource = (HTTPSSource)_datasetContext.GetById<DataSource>(source.Id);

                updatedSource.CurrentToken = _encryptService.EncryptString(newToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey).Item1;
                updatedSource.CurrentTokenExp = tokenExpTime;

                _datasetContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save new OAuthToken", ex);
                return false;
            }
        }

        public DataSourceDto GetDataSourceDto(int Id)
        {
            DataSourceDto dto = new DataSourceDto();
            DataSource dsrc = _datasetContext.GetById<DataSource>(Id);
            MapToDto(dsrc, dto);
            return dto;
        }
        #region PrivateMethods
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
        private void MapToDto(DataSource dsrc, DataSourceDto dto)
        {
            dto.OriginatingId = dsrc.Id;
            dto.Name = dsrc.Name;
            dto.Description = dsrc.Description;
            dto.RetrunUrl = null;
            dto.SourceType = dsrc.SourceType;
            dto.AuthID = dsrc.SourceAuthType.AuthID.ToString();
            dto.IsUserPassRequired = dsrc.IsUserPassRequired;
            dto.PortNumber = dsrc.PortNumber;
            dto.BaseUri = dsrc.BaseUri;

            MapDataSourceSpecificToDto(dsrc, dto);
        }

        private void MapDataSourceSpecificToDto(DataSource dsrc, DataSourceDto dto)
        {
            if (dsrc.Is<HTTPSSource>())
            {
                dto.ClientId = ((HTTPSSource)dsrc).ClientId;
                dto.ClientPrivateId = ((HTTPSSource)dsrc).ClientPrivateId;
                dto.TokenUrl = ((HTTPSSource)dsrc).TokenUrl;
                dto.TokenExp = ((HTTPSSource)dsrc).TokenExp;
                dto.Scope = ((HTTPSSource)dsrc).Scope;
                dto.RequestHeaders = ((HTTPSSource)dsrc).RequestHeaders;
                dto.TokenAuthHeader = ((HTTPSSource)dsrc).AuthenticationHeaderName;
                dto.UrlBasedRequest = ((HTTPSSource)dsrc).UrlBasedRequest;
                dto.ObjectBasedRequest = ((HTTPSSource)dsrc).ObjectBasedRequest;
            }
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

                    r.DataType = (!String.IsNullOrEmpty(b.DataType)) ? b.DataType.ToUpper() : "VARCHAR";
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
        #endregion
    }
}
