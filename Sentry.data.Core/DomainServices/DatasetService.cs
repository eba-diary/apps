using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.Entities.S3;

namespace Sentry.data.Core
{
    public class DatasetService : IDatasetService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly ISecurityService _securityService;
        private readonly UserService _userService;
        private readonly IConfigService _configService;
        private readonly ISchemaService _schemaService;
        private readonly IAWSLambdaProvider _awsLambdaProvider;

        public DatasetService(IDatasetContext datasetContext, ISecurityService securityService, 
                            UserService userService, IConfigService configService, 
                            ISchemaService schemaService, IAWSLambdaProvider awsLambdaProvider)
        {
            _datasetContext = datasetContext;
            _securityService = securityService;
            _userService = userService;
            _configService = configService;
            _schemaService = schemaService;
            _awsLambdaProvider = awsLambdaProvider;
        }


        public DatasetDto GetDatasetDto(int id)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == id && x.CanDisplay).FetchAllChildren(_datasetContext).FirstOrDefault();
            DatasetDto dto = new DatasetDto();
            MapToDto(ds, dto);

            return dto;
        }

        public DatasetDetailDto GetDatesetDetailDto(int id)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == id && x.CanDisplay).FetchAllChildren(_datasetContext).FirstOrDefault();

            if (ds != null)
            {
                DatasetDetailDto dto = new DatasetDetailDto();
                MapToDetailDto(ds, dto);
                return dto;
            }
            else
            {
                return null;
            }            
        }

        public List<DatasetDto> GetAllDatasetDto()
        {
            List<Dataset> dsList = _datasetContext.Datasets.Where(x => x.CanDisplay && x.DatasetType == "DS").FetchAllChildren(_datasetContext).ToList();
            List<DatasetDto> dtoList = new List<DatasetDto>();
            foreach (Dataset ds in dsList)
            {
                DatasetDto dto = new DatasetDto();
                MapToDto(ds, dto);
                dtoList.Add(dto);
            }
            return dtoList;
        }

        public UserSecurity GetUserSecurityForDataset(int datasetId)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == datasetId && x.CanDisplay).FetchSecurityTree(_datasetContext).FirstOrDefault();

            return _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
        }

        public UserSecurity GetUserSecurityForConfig(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.DatasetFileConfigs.Where(x => x.ConfigId == configId).FetchSecurityTree(_datasetContext).FirstOrDefault();
            if (dfc.ParentDataset != null)
            {
                return _securityService.GetUserSecurity(dfc.ParentDataset, _userService.GetCurrentUser());
            }
            return new UserSecurity();
        }

        public List<Dataset> GetDatasetsForQueryTool()
        {
            //get all datasets where the there is a CanQueryData permission on the security
            //OR all public datasets (no security object)
            var query = _datasetContext.Datasets.Where(x => x.DatasetType == GlobalConstants.DataEntityCodes.DATASET && x.CanDisplay);

            query.FetchMany(x => x.DatasetCategories).ToFuture();
            List<Dataset> datasets = query.FetchSecurityTree(_datasetContext);

            IApplicationUser user = _userService.GetCurrentUser();

            List<Dataset> datasetsCanQuery = new List<Dataset>();

            foreach (Dataset ds in datasets)
            {
                var userSecurity = _securityService.GetUserSecurity(ds, user);
                if (userSecurity.CanQueryDataset)
                {
                    datasetsCanQuery.Add(ds);
                }
            }

            //now add in all the public datasets.

            return datasetsCanQuery;
        }

        public AccessRequest GetAccessRequest(int datasetId)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);

            AccessRequest ar = new AccessRequest()
            {
                Permissions = _datasetContext.Permission.Where(x => x.SecurableObject == GlobalConstants.SecurableEntityName.DATASET).ToList(),
                ApproverList = new List<KeyValuePair<string, string>>(),
                SecurableObjectId = ds.DatasetId,
                SecurableObjectName = ds.DatasetName
            };


            IApplicationUser primaryUser = _userService.GetByAssociateId(ds.PrimaryOwnerId);
            ar.ApproverList.Add(new KeyValuePair<string, string>(ds.PrimaryOwnerId, primaryUser.DisplayName + " (Owner)"));

            if (!string.IsNullOrWhiteSpace(ds.PrimaryContactId))
            {
                IApplicationUser secondaryUser = _userService.GetByAssociateId(ds.PrimaryContactId);
                ar.ApproverList.Add(new KeyValuePair<string, string>(ds.PrimaryContactId, secondaryUser.DisplayName + " (Contact)"));
            }

            return ar;
        }

        public string RequestAccessToDataset(AccessRequest request)
        {

            Dataset ds = _datasetContext.GetById<Dataset>(request.SecurableObjectId);
            if (ds != null)
            {
                IApplicationUser user = _userService.GetCurrentUser();
                request.SecurableObjectName = ds.DatasetName;
                request.SecurityId = ds.Security.SecurityId;
                request.RequestorsId = user.AssociateId;
                request.RequestorsName = user.DisplayName;
                request.IsProd = bool.Parse(Configuration.Config.GetHostSetting("RequireApprovalHPSMTickets"));
                request.RequestedDate = DateTime.Now;
                request.ApproverId = request.SelectedApprover;
                request.Permissions = _datasetContext.Permission.Where(x => request.SelectedPermissionCodes.Contains(x.PermissionCode) &&
                                                                                                                x.SecurableObject == GlobalConstants.SecurableEntityName.DATASET).ToList();

                return _securityService.RequestPermission(request);
            }

            return string.Empty;
        }

        public int CreateAndSaveNewDataset(DatasetDto dto)
        {
            Dataset ds = CreateDataset(dto);
            _datasetContext.Add(ds);
            dto.DatasetId = ds.DatasetId;

            DatasetFileConfigDto configDto = dto.ToConfigDto();
            FileSchemaDto fileDto = dto.ToSchemaDto();
            configDto.SchemaId = _schemaService.CreateAndSaveSchema(fileDto);
            _configService.CreateAndSaveDatasetFileConfig(configDto);


            _datasetContext.SaveChanges();

            return ds.DatasetId;
        }

        public void UpdateAndSaveDataset(DatasetDto dto)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(dto.DatasetId);

            ds.DatasetInformation = dto.DatasetInformation;
            ds.OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), dto.OriginationId);
            ds.ChangedDtm = DateTime.Now;

            if (dto.DatasetCategoryIds?.Count() > 0)
            {
                ds.DatasetCategories = _datasetContext.Categories.Where(x => dto.DatasetCategoryIds.Contains(x.Id)).ToList();
            }
            if (null != dto.CreationUserId && dto.CreationUserId.Length > 0)
            {
                ds.CreationUserName = dto.CreationUserId;
            }
            if (null != dto.DatasetDesc && dto.DatasetDesc.Length > 0)
            {
                ds.DatasetDesc = dto.DatasetDesc;
            }
            if (dto.DatasetDtm > DateTime.MinValue)
            {
                ds.DatasetDtm = dto.DatasetDtm;
            }
            if (null != dto.PrimaryOwnerId && dto.PrimaryOwnerId.Length > 0)
            {
                ds.PrimaryOwnerId = dto.PrimaryOwnerId;
            }
            if (null != dto.PrimaryContactId && dto.PrimaryContactId.Length > 0)
            {
                ds.PrimaryContactId = dto.PrimaryContactId;
            }
            if (dto.DataClassification > 0)
            {
                ds.DataClassification = dto.DataClassification;
            }

            //override the Dto.IsSecured for certain classifications.
            switch (dto.DataClassification)
            {
                case GlobalEnums.DataClassificationType.HighlySensitive:
                    dto.IsSecured = true;
                    break;
                case GlobalEnums.DataClassificationType.InternalUseOnly:
                    //don't override as it should flow from the form.
                    break;
                default:
                    dto.IsSecured = false;
                    break;
            }

            if (!ds.IsSecured && dto.IsSecured)
            {
                if (ds.Security == null)
                {
                    ds.Security = new Security(GlobalConstants.SecurableEntityName.DATASET)
                    {
                        CreatedById = _userService.GetCurrentUser().AssociateId
                    };
                }
                else
                {
                    ds.Security.EnabledDate = DateTime.Now;
                    ds.Security.UpdatedById = _userService.GetCurrentUser().AssociateId;
                }
            }
            else if (ds.IsSecured && !dto.IsSecured)
            {
                ds.Security.RemovedDate = DateTime.Now;
                ds.Security.UpdatedById = _userService.GetCurrentUser().AssociateId;
            }

            ds.IsSecured = dto.IsSecured;


            _datasetContext.SaveChanges();
        }

        public bool Delete(int datasetId, bool logicalDelete = true)
        {            
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);

            if (logicalDelete)
            {
                Logger.Info($"datasetservice-delete-logical - datasetid:{datasetId} datasetname:{ds.DatasetName}");

                try
                {
                    //Mark dataset for soft delete
                    MarkForDelete(ds);

                    //Remove any favorite links to ensure users do not get dead link
                    foreach(var fav in ds.Favorities)
                    {
                        _datasetContext.RemoveById<Favorite>(fav.FavoriteId);
                    }

                    //Remove any notification subscriptions to dataset
                    foreach (var subscib in _datasetContext.GetSubscriptionsForDataset(ds.DatasetId))
                    {
                        _datasetContext.RemoveById<DatasetSubscription>(subscib.ID);
                    }
                    
                    //Mark Configs for soft delete to ensure no editing and jobs are disabled
                    foreach (DatasetFileConfig config in ds.DatasetFileConfigs)
                    {
                        _configService.Delete(config.ConfigId, logicalDelete, true);
                    }

                    _datasetContext.SaveChanges();

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error($"datasetservice-delete-logical failed", ex);
                    return false;
                }
                    
            }
            else
            {
                Logger.Info($"datasetservice-delete-physical - datasetid:{datasetId} datasetname:{ds.DatasetName}");

                try
                {
                    foreach (DatasetFileConfig config in ds.DatasetFileConfigs)
                    {
                        _configService.Delete(config.ConfigId, logicalDelete, true);
                    }

                    _datasetContext.RemoveById<Dataset>(ds.DatasetId);
                    _datasetContext.SaveChanges();

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error($"datasetservice-delete failed", ex);
                    return false;
                }                    
            }
            
        }
        
        public List<string> Validate(DatasetDto dto)
        {
            List<string> errors = new List<string>();
            if (dto.DatasetId == 0 && _datasetContext.Datasets.Where(w => w.DatasetName == dto.DatasetName &&
                                                                         w.DatasetCategories.Any(x => dto.DatasetCategoryIds.Contains(x.Id)) &&
                                                                         w.DatasetType == GlobalConstants.DataEntityCodes.DATASET).Count() > 0)
            {
                errors.Add("Dataset name already exists within category");
            }

            if (String.IsNullOrWhiteSpace(dto.PrimaryOwnerId))
            {
                errors.Add("Owner is requried.");
            }

            if (String.IsNullOrWhiteSpace(dto.PrimaryContactId))
            {
                errors.Add("Contact is requried.");
            }

            if (dto.DatasetCategoryIds.Count == 1 && dto.DatasetCategoryIds[0].Equals(0))
            {
                errors.Add("Category is required");
            }

            if (dto.DatasetId == 0 && dto.DatasetScopeTypeId == 0)
            {
                errors.Add("Dataset Scope is required");
            }

            return errors;
        }

        public List<Dataset> GetDatasetMarkedDeleted()
        {
            List<Dataset> dsList = _datasetContext.Datasets.Where(w => w.DeleteInd && w.DeleteIssueDTM < DateTime.Now.AddDays(Double.Parse(Configuration.Config.GetHostSetting("DatasetDeleteWaitDays")))).ToList();
            return dsList;
        }

        public void GenerateDatasetFilePreview(DatasetFile df)
        {
            //Failure to generate preview files should not hold up any processing
            try
            {
                LambdaClientConfig lConfig = new LambdaClientConfig()
                {
                    AWSRegion = Configuration.Config.GetSetting("AWSRegion"),
                    AccessKey = Configuration.Config.GetHostSetting("AWSAccessKey"),
                    SecretKey = Configuration.Config.GetHostSetting("AWSSecretKey"),
                    ProxyHost = Configuration.Config.GetHostSetting("SentryAWSLambdaProxyHost"),
                    ProxyPort = Configuration.Config.GetHostSetting("SentryAWSLambdaProxyPort")
                };

                _awsLambdaProvider.ConfigureClient(lConfig);
                _awsLambdaProvider.SetFunctionName(Configuration.Config.GetHostSetting("AWSPreviewLambdaName"));
                _awsLambdaProvider.SetInvocationType("RequestResponse");
                _awsLambdaProvider.SetLogType("Tail");
                _awsLambdaProvider.InvokeFunction(GeneratePreviewLambdaTriggerEvent(Configuration.Config.GetHostSetting("AWSRootBucket"), df));
            }
            catch (Exception ex)
            {
                Logger.Error("datasetservice-generatedatasetfilepreview failed to generate preview", ex);
            }
        }

        //public bool DeleteDatasetFile(int datasetFileId)
        //{
        //    DatasetFile df = _datasetContext.DatasetFile.Where(w => w.DatasetFileId == datasetFileId).FirstOrDefault();

        //    if (df != null)
        //    {
        //        try
        //        {
        //            //Find associated parquet prefixes and delete from S3
        //            List<DatasetFileParquet> parquetFileList = _datasetContext.DatasetFileParquet.Where(w => w.DatasetFileId == df.DatasetFileId).ToList();
        //            List<string> s3PrefixList = parquetFileList.ToObjectKeyVersion();
        //            _s3ServiceProvider.DeleteS3Prefix(s3PrefixList);


        //            _datasetContext.Remove(df);
        //            _datasetContext.SaveChanges();
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {

        //            Logger.Error($"deletedatasetfile-")
        //            return false;
        //        }

        //    }
        //    else
        //    {
        //        Logger.Info($"deletedatasetfile-nofilefound = datasetfileid:{datasetFileId}");
        //        return true;
        //    }
        //}

        #region "private functions"
        private void MarkForDelete(Dataset ds)
        {
            ds.CanDisplay = false;
            ds.DeleteInd = true;
            ds.DeleteIssuer = _userService.GetCurrentUser().AssociateId;
            ds.DeleteIssueDTM = DateTime.Now;
        }

        private Dataset CreateDataset(DatasetDto dto)
        {
            Dataset ds = new Dataset()
            {
                DatasetId = dto.DatasetId,
                DatasetCategories = _datasetContext.Categories.Where(x => x.Id == dto.DatasetCategoryIds.First()).ToList(),
                DatasetName = dto.DatasetName,
                DatasetDesc = dto.DatasetDesc,
                DatasetInformation = dto.DatasetInformation,
                CreationUserName = dto.CreationUserId,
                PrimaryOwnerId = dto.PrimaryOwnerId,
                PrimaryContactId = dto.PrimaryContactId,
                UploadUserName = dto.UploadUserId,
                OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), dto.OriginationId),
                DatasetDtm = dto.DatasetDtm,
                ChangedDtm = dto.ChangedDtm,
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DataClassification = dto.DataClassification,
                IsSecured = dto.IsSecured,
                CanDisplay = true,
                DatasetFiles = null,
                DatasetFileConfigs = null,
                DeleteInd = false,
                DeleteIssueDTM = DateTime.MaxValue
            };

            switch (dto.DataClassification)
            {
                case GlobalEnums.DataClassificationType.HighlySensitive:
                    ds.IsSecured = true;
                    break;
                case GlobalEnums.DataClassificationType.InternalUseOnly:
                    ds.IsSecured = dto.IsSecured;
                    break;
                default:
                    ds.IsSecured = false;
                    break;
            }

            if (ds.IsSecured)
            {
                ds.Security = new Security(GlobalConstants.SecurableEntityName.DATASET)
                {
                    CreatedById = _userService.GetCurrentUser().AssociateId
                };
            }

            return ds;
        }

        private void MapToDto(Dataset ds, DatasetDto dto)
        {
            IApplicationUser primaryOwner = _userService.GetByAssociateId(ds.PrimaryOwnerId);
            IApplicationUser primaryContact = _userService.GetByAssociateId(ds.PrimaryContactId);
            IApplicationUser uploader = _userService.GetByAssociateId(ds.UploadUserName);

            //map the ISecurable properties
            dto.Security = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
            dto.PrimaryOwnerId = ds.PrimaryOwnerId;
            dto.PrimaryContactId = ds.PrimaryContactId;
            dto.IsSecured = ds.IsSecured;

            dto.DatasetId = ds.DatasetId;
            dto.DatasetCategoryIds = ds.DatasetCategories.Select(x => x.Id).ToList();
            dto.DatasetName = ds.DatasetName;
            dto.DatasetDesc = ds.DatasetDesc;
            dto.DatasetInformation = ds.DatasetInformation;
            dto.DatasetType = ds.DatasetType;
            dto.DataClassification = ds.DataClassification;
            dto.CategoryColor = ds.DatasetCategories.FirstOrDefault().Color;

            dto.CreationUserId = ds.CreationUserName;
            dto.CreationUserName = ds.CreationUserName;
            dto.PrimaryOwnerName = (primaryOwner != null ? primaryOwner.DisplayName : "Not Available");
            dto.PrimaryContactName = (primaryContact != null ? primaryContact.DisplayName : "Not Available");
            dto.PrimaryContactEmail = (primaryContact != null ? primaryContact.EmailAddress : "");
            dto.UploadUserId = ds.UploadUserName;
            dto.UploadUserName = (uploader != null ? uploader?.DisplayName : "Not Available");

            dto.DatasetDtm = ds.DatasetDtm;
            dto.ChangedDtm = ds.ChangedDtm;
            dto.CanDisplay = ds.CanDisplay;
            dto.TagIds = new List<string>();
            dto.OriginationId = (int)Enum.Parse(typeof(DatasetOriginationCode), ds.OriginationCode);
            dto.ConfigFileDesc = ds.DatasetFileConfigs?.First()?.Description;
            dto.ConfigFileName = ds.DatasetFileConfigs?.First()?.Name;
            dto.Delimiter = ds.DatasetFileConfigs?.First()?.Schema?.Delimiter;
            dto.FileExtensionId = ds.DatasetFileConfigs.First().FileExtension.Id;
            dto.DatasetScopeTypeId = ds.DatasetFileConfigs.First().DatasetScopeType.ScopeTypeId;
            dto.CategoryName = ds.DatasetCategories.First().Name;
            dto.MailtoLink = "mailto:?Subject=Dataset%20-%20" + ds.DatasetName + "&body=%0D%0A" + Configuration.Config.GetHostSetting("SentryDataBaseUrl") + "/Dataset/Detail/" + ds.DatasetId;
            dto.CategoryNames = ds.DatasetCategories.Select(s => s.Name).ToList();
        }



        private void MapToDetailDto(Dataset ds, DatasetDetailDto dto)
        {
            MapToDto(ds, dto);

            IApplicationUser user = _userService.GetCurrentUser();

            dto.Downloads = _datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.DOWNLOAD && x.Dataset == ds.DatasetId).Count();
            dto.IsSubscribed = _datasetContext.IsUserSubscribedToDataset(_userService.GetCurrentUser().AssociateId, dto.DatasetId);
            dto.AmountOfSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(_userService.GetCurrentUser().AssociateId, dto.DatasetId).Count;
            dto.Views = _datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.VIEWED && x.Dataset == ds.DatasetId).Count();
            dto.IsFavorite = ds.Favorities.Any(w => w.UserId == user.AssociateId);
            dto.DatasetFileConfigNames = ds.DatasetFileConfigs.Where(w => w.DeleteInd == false).ToDictionary(x => x.ConfigId.ToString(), y => y.Name);
            dto.DatasetScopeTypeNames = ds.DatasetScopeType.ToDictionary(x => x.Name, y => y.Description);
            dto.DistinctFileExtensions = ds.DatasetFiles.Select(x => Path.GetExtension(x.FileName).TrimStart('.').ToLower()).Distinct().ToList();
            dto.DatasetFileCount = ds.DatasetFiles.Count();
            dto.OriginationCode = ds.OriginationCode;
            dto.DataClassificationDescription = ds.DataClassification.GetDescription();
            dto.CategoryColor = ds.DatasetCategories.First().Color;
            dto.CategoryNames = ds.DatasetCategories.Select(x => x.Name).ToList();
            dto.GroupAccessCount = _securityService.GetGroupAccessCount(ds);
            if (ds.DatasetFiles.Any())
            {
                dto.ChangedDtm = ds.DatasetFiles.Max(x => x.ModifiedDTM);
            }
        }

        private string GeneratePreviewLambdaTriggerEvent(string bucket, DatasetFile dsf)
        {
            S3LamdaEvent lambdaEvent = new S3LamdaEvent()
            {
                Records = new List<S3ObjectEvent>()
                {
                    new S3ObjectEvent()
                    {
                        eventName = "ObjectCreated:Put",
                        s3 = new S3()
                        {
                            bucket = new Bucket()
                            {
                                name = bucket
                            },
                            Object = new data.Core.Entities.S3.Object()
                            {
                                key = dsf.FileLocation
                            }
                        }
                    }
                }
            };

            return JsonConvert.SerializeObject(lambdaEvent);
        }

        #endregion

    }
}
