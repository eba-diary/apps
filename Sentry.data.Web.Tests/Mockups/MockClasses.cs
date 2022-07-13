using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Tests
{
    public static class MockClasses
    {
        public static DataFlowMetricEntity MockDataFlowMetricEntity()
        {
            DataFlowMetricEntity entity = new DataFlowMetricEntity
            {
                QueryMadeDateTime = DateTime.Now,
                SchemaId = 0,
                EventContents = "contents",
                TotalFlowSteps = 0,
                FileModifiedDateTime = DateTime.Now,
                OriginalFileName = "name",
                DatasetId = 0,
                CurrentFlowStep = 0,
                DataActionId = 0,
                DataFlowId = 0,
                Partition = 0,
                DataActionTypeId = 0,
                MessageKey = 0,
                Duration = 0,
                Offset = 0,
                DataFlowName = "name",
                DataFlowStepId = 0,
                FlowExecutionGuid = "guid",
                FileSize = 0,
                EventMetricId = 0,
                StorageCode = "code",
                FileCreatedDateTime = DateTime.Now,
                RunInstanceGuid = 0,
                FileName = "name",
                SaidKeyCode = "code",
                MetricGeneratedDateTime = DateTime.Now,
                DatesetFileId = 0,
                ProcessStartDateTime = DateTime.Now,
                StatusCode = "code"
            };
            return entity;
        }
        public static Dataset MockDataset(IApplicationUser user = null, Boolean addConfig = false)
        {
            Dataset ds = new Dataset()
            {
                DatasetId = 1000,
                DatasetCategories = MockCategories(),
                DatasetName = "Claim Dataset",
                DatasetDesc = "Test Claim Dataset",
                DatasetInformation = "Specific Information regarding datasetfile consumption",
                CreationUserName = user != null ? user.DisplayName : "Nye, Bill",
                UploadUserName = user != null ? user.DisplayName : "Nye, Bill",
                OriginationCode = "Internal",
                DatasetDtm = System.DateTime.Now.AddYears(-13),
                ChangedDtm = System.DateTime.Now.AddYears(-12),
                S3Key = "data-dev/government/quarterly_census_of_employment_and_wages/",
                CanDisplay = true,
                DatasetFiles = new List<DatasetFile>(),
                DatasetFileConfigs = new List<DatasetFileConfig>(),
                Favorities = new List<Favorite>()
            };

            if (addConfig)
            {
                ds.DatasetFileConfigs.Add(MockDataFileConfig(ds));
            }

            return ds;
        }

        public static List<Category> MockCategories()
        {
            Category c = new Category()
            {
                Id = 1,
                Name = "Claim",
                Color = "Gold"
            };

            List<Category> categories = new List<Category>();
            categories.Add(c);

            return categories;
        }


        public static DatasetFileConfig MockDataFileConfig(Dataset ds = null)
        {            
            DatasetFileConfig dfc = new DatasetFileConfig()
            {
                ConfigId = 2000,
                Name = "Default",
                Description = "Default Config for Dataset.  Uploaded files that do not match any configs will default to this config",
                FileTypeId = (int)FileType.DataFile,
                ParentDataset = ds != null ? ds : MockDataset(),
                DatasetScopeType = MockScopeTypes()[0],
                FileExtension = MockFileExtensions()[0],
                RetrieverJobs = new List<RetrieverJob>(),                
            };

            //ist<DataElement> deList = new List<DataElement>();
            //deList.Add(MockDataElement(dfc));

            //dfc.Schemas = deList;

            return dfc;
        }

        //public static DataElement MockDataElement(DatasetFileConfig dfc = null)
        //{
        //    DatasetFileConfig dsfc = dfc != null ? dfc : MockDataFileConfig();

        //    DataElement de = new DataElement()
        //    {
        //        DataElementCreate_DTM = DateTime.Now,
        //        DataElementChange_DTM = DateTime.Now,
        //        DataElement_CDE = "F",
        //        DataElement_DSC = GlobalConstants.DataElementDescription.DATA_FILE,
        //        DataElement_NME = dsfc.Name,
        //        LastUpdt_DTM = DateTime.Now,
        //        SchemaIsPrimary = true,
        //        SchemaDescription = dsfc.Description,
        //        SchemaName = dsfc.Name,
        //        SchemaRevision = 1,
        //        SchemaIsForceMatch = false,
        //        FileFormat = MockFileExtensions()[0].Name.ToUpper(),
        //        StorageCode = "1000123",
        //        DatasetFileConfig = dsfc
        //    };

        //    return de;
        //}

        public static List<DatasetScopeType> MockScopeTypes()
        {
            DatasetScopeType dst = new DatasetScopeType()
            {
                ScopeTypeId = 1,
                Name = "Point-in-Time",
                Description = "Transactional data",
                IsEnabled = true
            };

            List<DatasetScopeType> datasetScopeTypes = new List<DatasetScopeType>();
            datasetScopeTypes.Add(dst);

            return datasetScopeTypes;
        }

        public static List<FileExtension> MockFileExtensions()
        {
            FileExtension fe = new FileExtension()
            {
                Id = 2,
                Name = GlobalConstants.ExtensionNames.CSV,
                Created = System.DateTime.Now.AddYears(-13),
                CreatedUser = "072984"
            };

            List<FileExtension> feList = new List<FileExtension>();

            feList.Add(fe);

            return feList;
        }

        public static List<DataFlowStepDto> MockDataFlowSteoDtos(int listSize)
        {
            List<DataFlowStepDto> steps = new List<DataFlowStepDto>();
            for (int i = 1; i <= listSize; i++)
            {
                steps.Add(new DataFlowStepDto()
                {
                    Id = i,
                    DataFlowId = i,
                    DataFlowName = "DataFlowName" + i,
                    DataActionType = DataActionType.None,
                    DataActionTypeId = i,
                    DataActionName = "DataActionName" + i,
                    ActionId = i,
                    ActionName = "ActionName" + i,
                    ActionDescription = "ActionDescription",
                    ExeuctionOrder = i,
                    TriggerKey = "TriggerKey" + i,
                    TriggerBucket = "TriggerBucket",
                    TargetPrefix = "TargetPrefix"
                });
            }

            return steps;
        }

        public static List<DataFlowDetailDto> MockDataFlowDetailDtos(int listSize)
        {
            List<DataFlowDetailDto> dtos = new List<DataFlowDetailDto>();
            for (int i = 1; i<=listSize; i++)
            {
                dtos.Add(new DataFlowDetailDto()
                {
                    Id = i,
                    FlowGuid = Guid.NewGuid(),
                    SaidKeyCode = "SaidKeyCode",
                    DatasetId = 1,
                    SchemaId = 1,
                    Name = "Name",
                    CreateDTM = DateTime.Now,
                    CreatedBy = "CreatedBy",
                    DFQuestionnaire = "DFQuestionnaire",
                    IngestionType = 1,
                    IsCompressed = true,
                    IsPreProcessingRequired = true,
                    FlowStorageCode = "FlowStorageCode",
                    MappedSchema = new List<int> { 1, 2 },
                    AssociatedJobs = new List<int> { 1, 2 },
                    ObjectStatus = ObjectStatusEnum.Active,
                    DeleteIssuer = "DeleteIssuer",
                    DeleteIssueDTM = DateTime.Now,
                    NamedEnvironment = "NamedEnvironment",
                    NamedEnvironmentType = NamedEnvironmentType.Prod
                });
            }

            return dtos;
        }

        public static DataFlowDetailDto MockDataFlowDetailDto()
        {
            DataFlowDetailDto dto = new DataFlowDetailDto()
            {
                Id = 1,
                FlowGuid = Guid.NewGuid(),
                SaidKeyCode = "SaidKeyCode",
                DatasetId = 1,
                SchemaId = 1,
                Name = "Name",
                CreateDTM = DateTime.Now,
                CreatedBy = "CreatedBy",
                DFQuestionnaire = "DFQuestionnaire",
                IngestionType = 1,
                IsCompressed = true,
                IsPreProcessingRequired = true,
                FlowStorageCode = "FlowStorageCode",
                MappedSchema = new List<int> { 1, 2 },
                AssociatedJobs = new List<int> { 1, 2 },
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssuer = "DeleteIssuer",
                DeleteIssueDTM = DateTime.Now,
                NamedEnvironment = "NamedEnvironment",
                NamedEnvironmentType = NamedEnvironmentType.Prod
            };

            return dto;
        }

        public static List<DataSource> MockDataSources()
        {
            List<DataSource> dataSources = new List<DataSource>();

            DfsBasic dfs = new DfsBasic();

            dfs.Name = "Default Drop Location";
            dfs.Description = "Default Drop Location";

            dataSources.Add(dfs);

            S3Basic s3 = new S3Basic();

            s3.Name = "Default S3 Drop Location";
            s3.Description = "Default S3 Drop Location";
            s3.Bucket = "sentry-dataset-management-np-nr";
            s3.BaseUri = new Uri("http://s3-sa-east-1.amazonaws.com/sentry-dataset-management-np-nr/data-dev/droplocation");

            dataSources.Add(s3);

            FtpSource ftp = new FtpSource();

            ftp.Name = "CAB";
            ftp.Description = "CAB";
            ftp.BaseUri = new Uri("ftp://ftp.cabfinancial.com/");

            dataSources.Add(ftp);

            return dataSources;
        }


        public static DataSource MockDataSource(DataSource dsrc, AuthenticationType authIn = null)
        {
            AuthenticationType authenticationType = new AnonymousAuthentication()
            {
                AuthID = 1,
                AuthName = "Anonymous Authentication",
                Description = "Provides generic credentials with the following properties: User=\"anonymous\" and Password=(generic email address)",
                IsUserPassRequired = false
            };

            if (dsrc != null)
            {
                if (dsrc.Is<DfsBasic>())
                {
                    dsrc.Name = "Basic_Name";
                    dsrc.Description = "Basic_Description";
                    return dsrc;
                }
                else if (dsrc.Is<DfsCustom>())
                {
                    dsrc.Name = "Custom_Name";
                    dsrc.Description = "Custom_Description";
                    return dsrc;
                }
                else if (dsrc.Is<FtpSource>())
                {
                    dsrc.SourceAuthType = authIn != null ? authIn : authenticationType;
                    dsrc.BaseUri = new Uri(@"ftp://ftp.Sentry.com/");
                    return dsrc;
                }
                else
                {
                    DfsBasic n = new DfsBasic();
                    n.Name = "Basic_Name";
                    n.Description = "Basic_Description";
                    return n;
                }
            }
            else
            {
                DfsBasic n = new DfsBasic();
                n.Name = "Basic_Name";
                n.Description = "Basic_Description";
                return n;
            }            
        }

        public static DatasetFile MockDataFile(Dataset ds, DatasetFileConfig dfc, IApplicationUser user)
        {
            DatasetFile df = new DatasetFile()
            {
                DatasetFileId = 3000,
                FileName = "2014.annual.singlefile.csv",
                Dataset = ds,
                UploadUserName = user.AssociateId,
                CreatedDTM = System.DateTime.Now.AddYears(-12),
                ModifiedDTM = System.DateTime.Now.AddYears(-12),
                FileLocation = "data-dev/government/quarterly_census_of_employment_and_wages/235/2018/1/18/2014.annual.singlefile.csv",
                DatasetFileConfig = dfc,
                IsBundled = false         
            };

            return df;
        }

        public static RetrieverJob GetMockRetrieverJob(DatasetFileConfig dfc = null, DataSource dsrc = null, AuthenticationType authType = null)
        {
            Compression compression = new Compression()
            {
                IsCompressed = false,
                CompressionType = null,
                FileNameExclusionList = new List<string>()
            };

            RetrieverJobOptions rjo = new RetrieverJobOptions()
            {
                OverwriteDataFile = false,
                TargetFileName = "",
                CreateCurrentFile = false,
                IsRegexSearch = true,
                SearchCriteria = "\\.",
                CompressionOptions = compression
            };

            RetrieverJob rj = new RetrieverJob()
            {
                Id = 4000,
                Schedule = "Instant",
                TimeZone = "Central Standard Time",
                RelativeUri = null,

                DataSource = MockDataSource(dsrc, authType),

                DatasetConfig = dfc != null ? dfc : MockDataFileConfig(null),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                IsGeneric = true,

                JobOptions = rjo,
                ObjectStatus = ObjectStatusEnum.Active
            };

            if (rj.DataSource.Is<DfsBasic>())
            {
                //Valid type, but no action needed
            }
            else if (rj.DataSource.Is<DfsCustom>())
            {
                rj.RelativeUri = @"Custom\Directory\";
            }
            else if (rj.DataSource.Is<FtpSource>())
            {
                rj.RelativeUri = @"SourceFolder\CurrentDirectory\";
            }
            else
            {
                throw new NotImplementedException();
            }

            return rj;
        }

        public static DataAsset MockDataAsset()
        {
            DataAsset da = new DataAsset();
            da.Id = 1;
            da.Name = "MockAsset";
            da.DisplayName = "Mock Asset";
            da.Description = "This is a description";

            List<Notification> anList = new List<Notification>();
            anList.Add(GetMockAssetNotifications(da));

            da.AssetNotifications = anList;

            return da;
        }

        public static Notification GetMockAssetNotifications(DataAsset da, IApplicationUser user = null)
        {
            Notification an = new Notification();
            an.NotificationId = 1;
            an.ParentObject = da.Id;
            an.MessageSeverity = NotificationSeverity.Critical;
            an.Message = "Alert Message";
            an.StartTime = DateTime.Now.AddHours(-1).AddMinutes(1);
            an.ExpirationTime = DateTime.Now.AddDays(1);
            an.NotificationType = GlobalConstants.Notifications.DATAASSET_TYPE;
            an.Title = "Alert Title";

            if(user != null)
            {
                an.CreateUser = user.AssociateId;
            }

            return an;
        }

        public static List<EventType> MockEventTypes()
        {
            EventType et = new EventType();
            et.Description = "Viewed";
            et.Severity = 1;
            et.Display = false;
            et.Type_ID = 1;

            EventType et3 = new EventType();
            et3.Description = "Created File";
            et3.Severity = 1;         
            et3.Display = true;
            et3.Type_ID = 1;

            List<EventType> types = new List<EventType>();
            types.Add(et);
            types.Add(et3);

            return types;
        }

        public static List<Status> MockEventStatuses()
        {
            Status s = new Status();
            s.Description = "Success";
            s.Status_ID = 1;

            List<Status> statuses = new List<Status>();
            statuses.Add(s);
            return statuses;
        }

        public static List<Interval> MockIntervals()
        {
            List<Interval> intervals = new List<Interval>();

            Interval i = new Interval();
            i.Interval_ID = 1;
            i.Description = "Daily";

            intervals.Add(i);

            Interval i2 = new Interval();
            i2.Interval_ID = 2;
            i2.Description = "Never";

            intervals.Add(i2);

            return intervals;
        }

        public static List<DatasetSubscription> MockDatasetSubscriptions(Dataset ds, IApplicationUser user = null)
        {
            List<DatasetSubscription> datasetSubscriptions = new List<DatasetSubscription>();
            DatasetSubscription subscription = new DatasetSubscription();

            subscription.Dataset = ds;
            subscription.SentryOwnerName = user != null ? user.AssociateId : "012345";
            subscription.EventType = MockEventTypes()[0];
            subscription.Interval = MockIntervals()[0];
            subscription.ID = 0;

            datasetSubscriptions.Add(subscription);
            return datasetSubscriptions;
        }


        public static Event MockEvent()
        {
            return new Event
            {
                EventType = MockEventTypes()[0],
                Status = MockEventStatuses()[0],
                UserWhoStartedEvent = "012345"
            };
        }

        //public static List<Schema> MockSchemas(DatasetFileConfig dfc = null)
        //{
        //    Schema schema = new Schema()
        //    {
        //        Schema_NME = "Mock Schema",
        //        Schema_ID = 11000,
        //        Schema_DSC = "Mock Schema",
        //        Created_DTM = DateTime.Now,
        //        DatasetFileConfig = dfc
        //    };

        //    var schemas = new List<Schema>();

        //    schemas.Add(schema);

        //    return schemas;
           
        //}
    }
}
