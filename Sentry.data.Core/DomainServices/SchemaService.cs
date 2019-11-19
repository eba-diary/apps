using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;

namespace Sentry.data.Core
{
    public class SchemaService : ISchemaService
    {
        public IDatasetContext _datasetContext;
        public IUserService _userService;
        public IEmailService _emailService;

        public SchemaService(IDatasetContext dsContext, IUserService userService, IEmailService emailService)
        {
            _datasetContext = dsContext;
            _userService = userService;
            _emailService = emailService;
        }

        public int CreateAndSaveSchema(FileSchemaDto schemaDto)
        {
            int newSchemaId = 0;
            try
            {
                newSchemaId = CreateSchema(schemaDto);

                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("schemaservice-createandsaveschema", ex);
                return 0;
            }

            return newSchemaId;            
        }

        public bool UpdateAndSaveSchema(FileSchemaDto schemaDto)
        {
            
            var SendSASNotification = false;
            string SASNotificationType = null;
            string CurrentViewNotificationType = null;
            try
            {
                FileSchema schema = _datasetContext.GetById<FileSchema>(schemaDto.SchemaId);
                var SchemaRevisionExists = _datasetContext.SchemaRevision.Where(w => w.ParentSchema == schema).Any();

                #region SAS Notification Determination Logic
                //      This logic needs to be determine prior to mapping DTO to schema so change detection occurs properly
                //      Notification logic occurs after changes successfully saved to database

                /*
                 * Detect change within IsInSAS property when
                 *      Schema Revision exists
                 * if change,
                 *      set notification trigger to true
                 *      set type of notification
                 */
                if (SchemaRevisionExists && schema.IsInSAS != schemaDto.IsInSAS)
                {
                    SendSASNotification = true;
                    SASNotificationType = (schemaDto.IsInSAS) ? "ADD" : "REMOVE";
                }

                /*
                 * Determine change within CurrentView property when 
                 *      Schema Revision exists
                 *      IsInSAS is true or when IsInSAS has changed to false
                 * if change,
                 *      set notification trigger to true
                 *      set type of notification
                 */
                if (SchemaRevisionExists && (schemaDto.IsInSAS || SASNotificationType.ToUpper() == "REMOVE") && schema.CreateCurrentView != schemaDto.CreateCurrentView)
                {
                    SendSASNotification = true;
                    CurrentViewNotificationType = (schemaDto.CreateCurrentView) ? "ADD" : "REMOVE";
                }
                #endregion

                UpdateAndSaveSchema(schemaDto, schema);
                _datasetContext.SaveChanges();

                //Send notification to SAS
                if (SendSASNotification)
                {
                    SasNotification(schema, SASNotificationType, CurrentViewNotificationType);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("schemaservice-updateandsaveschema", ex);
                return false;
            }
        }

        private void UpdateAndSaveSchema(FileSchemaDto dto, FileSchema schema)
        {
            bool chgDetected = false;
            if (schema.Name != dto.Name)
            {
                schema.Name = dto.Name;
                chgDetected = true;
            };
            if (schema.Delimiter != dto.Delimiter)
            {
                schema.Description = dto.Description;
                chgDetected = true;
            }
            if (schema.CreateCurrentView != dto.CreateCurrentView)
            {
                schema.CreateCurrentView = dto.CreateCurrentView;
                chgDetected = true;
            }
            if (schema.Description != dto.Description)
            {
                schema.Description = dto.Description;
                chgDetected = true;
            }
            if (schema.Extension.Id != dto.FileExtensionId)
            {
                schema.Extension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId);
                chgDetected = true;
            }
            if (schema.HasHeader != dto.HasHeader)
            {
                schema.HasHeader = dto.HasHeader;
                chgDetected = true;
            }
            if (schema.IsInSAS != dto.IsInSAS)
            {
                schema.IsInSAS = dto.IsInSAS;
                chgDetected = true;
            }

            if (chgDetected)
            {
                schema.LastUpdatedDTM = DateTime.Now;
                schema.UpdatedBy = _userService.GetCurrentUser().AssociateId;
            } 
            
        }

        public FileSchemaDto GetFileSchemaDto(int id)
        {
            FileSchema scm = _datasetContext.FileSchema.Where(w => w.SchemaId == id).FirstOrDefault();
            return scm.MapToDto();
        }

        public SchemaRevisionDto GetSchemaRevisionDto(int id)
        {
            SchemaRevision revision = _datasetContext.GetById<SchemaRevision>(id);
            SchemaRevisionDto dto = revision.ToDto();
            return dto;
        }

        public List<SchemaRevisionDto> GetSchemaRevisionDtoBySchema(int id)
        {
            List<SchemaRevisionDto> dtoList = new List<SchemaRevisionDto>();
            foreach (SchemaRevision revision in _datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == id).ToList())
            {
                dtoList.Add(revision.ToDto());
            }
            return dtoList;
        }

        public List<BaseFieldDto> GetBaseFieldDtoBySchemaRevision(int revisionId)
        {
            SchemaRevision revision = _datasetContext.SchemaRevision.FirstOrDefault(w => w.SchemaRevision_Id == revisionId);

            return revision.Fields.Where(w => w.ParentField == null).OrderBy(o => o.OrdinalPosition).ToList().ToDto();
        }

        public SchemaRevisionDto GetLatestSchemaRevisionDtoBySchema(int schemaId)
        {
            SchemaRevision revision = _datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == schemaId).OrderByDescending(o => o.Revision_NBR).Take(1).FirstOrDefault();

            return revision.ToDto();
        }

        public List<DatasetFile> GetDatasetFilesBySchema(int schemaId)
        {
            List<DatasetFile> fileList = _datasetContext.DatasetFile.Where(w => w.Schema.SchemaId == schemaId).ToList();
            return fileList;
        }

        public DatasetFile GetLatestDatasetFileBySchema(int schemaId)
        {
            DatasetFile file = _datasetContext.DatasetFile.OrderBy(x => x.CreateDTM).FirstOrDefault(w => w.Schema.SchemaId == schemaId);
            return file;
        }

        private int CreateSchema(FileSchemaDto dto)
        {
            string storageCode = _datasetContext.GetNextStorageCDE().ToString();
            Dataset parentDataset = _datasetContext.GetById<Dataset>(dto.ParentDatasetId);
            FileSchema schema = new FileSchema()
            {
                Name = dto.Name,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                SchemaEntity_NME = dto.SchemaEntity_NME,
                Extension = (dto.FileExtensionId != 0) ? _datasetContext.GetById<FileExtension>(dto.FileExtensionId) : (dto.FileExtenstionName != null) ? _datasetContext.FileExtensions.Where(w => w.Name == dto.FileExtenstionName).FirstOrDefault() : null,
                Delimiter = dto.Delimiter,
                HasHeader = dto.HasHeader,
                IsInSAS = dto.IsInSAS,
                SasLibrary = CommonExtensions.GenerateSASLibaryName(_datasetContext.GetById<Dataset>(dto.ParentDatasetId)),
                Description = dto.Description,
                StorageCode = storageCode,
                HiveDatabase = "dsc_" + parentDataset.DatasetCategories.First().Name.ToLower(),
                HiveTable = parentDataset.DatasetName.Replace(" ", "").Replace("_", "").ToUpper() + "_" + dto.Name.Replace(" ", "").ToUpper(),
                HiveTableStatus = HiveTableStatusEnum.NameReserved.ToString(),
                HiveLocation = Configuration.Config.GetHostSetting("AWSRootBucket") + "/" + GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + storageCode,
                CreatedDTM = DateTime.Now,
                LastUpdatedDTM = DateTime.Now,
                DeleteIssueDTM = DateTime.MaxValue
            };
            _datasetContext.Add(schema);
            return schema.SchemaId;
        }

        public bool SasUpdateNotification(int schemaId, int revisionId)
        {
            SchemaRevision rev = null;
            try
            {
                rev = _datasetContext.SchemaRevision.Where(w => w.SchemaRevision_Id == revisionId && w.ParentSchema.SchemaId == schemaId).FirstOrDefault();
                bool fieldChanges = rev.Fields.Where(w => w.LastUpdateDTM == rev.LastUpdatedDTM).Any();
                if (fieldChanges && rev.Revision_NBR == 1)
                {
                    SasNotification(rev.ParentSchema, "ADD", null);
                }
                else if (fieldChanges)
                {
                    SasNotification(rev.ParentSchema, "UPDATE", null);
                }

                return true;
            }
            catch (Exception ex)
            {
                int revId = (rev != null) ? rev.SchemaRevision_Id : 0;
                Logger.Error($"Failed sending SAS email - revision:{revId}", ex);

                return false;
            }
        }


        private void SasNotification(FileSchema schema, string sasNotificationType, string currentViewNotificationType)
        {
            StringBuilder bodySb = new StringBuilder();
            string subject = null;
            IApplicationUser user = _userService.GetCurrentUser();
            //Ensure properties are initialized
            sasNotificationType = (sasNotificationType == null) ? string.Empty : sasNotificationType;
            currentViewNotificationType = (currentViewNotificationType == null) ? string.Empty : currentViewNotificationType;

            switch (sasNotificationType.ToUpper())
            {
                //Addition of all schema views to SAS
                case "ADD":
                    Logger.Debug($"Configuring SAS Notification to ADD all view(s)");
                    subject = $"Library Add Request to {schema.SasLibrary}";
                    bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be added to {schema.SasLibrary}:</p>");
                    bodySb.AppendLine($"<p>- vw_{schema.HiveTable}</p>");
                    //Include current view if checked
                    if (currentViewNotificationType == "ADD" || schema.CreateCurrentView)
                    {
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    break;
                //Removal of all schema views from SAS
                case "REMOVE":
                    Logger.Debug($"Configuring SAS Notification to REMOVE all view(s)");
                    subject = $"Library Remove Request from {schema.SasLibrary}";
                    bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be removed from {schema.SasLibrary}:</p>");
                    bodySb.AppendLine($"<p>- vw_{schema.HiveTable}</p>");
                    //if current view is being updated to unchecked or is currently checked, ensure it is removed from SAS
                    if (currentViewNotificationType.ToUpper() == "REMOVE" || schema.CreateCurrentView)
                    {
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    break;
                //Update of all SAS libraries
                case "UPDATE":
                    Logger.Debug($"Configuring SAS Notification to UDPATE all view(s)");
                    subject = $"Library Refresh Request from {schema.SasLibrary}";
                    bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be removed from {schema.SasLibrary}:</p>");
                    bodySb.AppendLine($"<p>- vw_{schema.HiveTable}</p>");
                    if (schema.CreateCurrentView)
                    {
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    break;
                //Current View propery can be changed independently of IsInSAS property
                //  Ensure notification is sent for current view propery changes if IsInSAS is checked
                default:
                    if (schema.IsInSAS && currentViewNotificationType.ToUpper() == "ADD")
                    {
                        Logger.Debug($"Configuring SAS Notification to ADD current view");
                        subject = $"Library Add Request from {schema.SasLibrary}";
                        bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be added to {schema.SasLibrary}:</p>");
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    else if (schema.IsInSAS && currentViewNotificationType.ToUpper() == "REMOVE")
                    {
                        Logger.Debug($"Configuring SAS Notification to REMOVE current view");
                        subject = $"Library Remove Request from {schema.SasLibrary}";
                        bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be removed from {schema.SasLibrary}:</p>");
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    break;
            }

            string ccEmailList = Configuration.Config.GetHostSetting("EmailDSCSupportAsCC") == "true" ? $"{user.EmailAddress};DSCSupport@sentry.com" : $"{user.EmailAddress}";

            if (bodySb.Length > 0)
            {
                bodySb.Append($"<p>Thank you from your friendly data.sentry.com Administration team</p>");

                _emailService.SendGenericEmail(Configuration.Config.GetHostSetting("SASAdministrationEmail"), subject, bodySb.ToString(), ccEmailList);

            }
            else
            {
                Logger.Warn($"SAS Notification was not configured");
            }
        }
    }
}
