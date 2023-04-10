using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Core.Tests
{
    public static class MockClasses
    {
        const string HrDatasetBucket = "sentry-dlst-dev-hrdroplocation-ae2";
        const string HrDatasetBucketNonProd = "sentry-dlst-qualnp-hrdroplocation-ae2";
        const string DlstDatasetBucket = "sentry-dlst-dev-droplocation-ae2";
        const string DlstDatasetBucketNonProd = "sentry-dlst-qualnp-droplocation-ae2";
        const string DataDatasetBucket = "sentry-data-dev-hrdroplocation-ae2";
        public static Dataset MockDataset(IApplicationUser user = null, Boolean addConfig = false, Boolean isSecured = false)
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
                Favorities = new List<Favorite>(),
                IsSecured = isSecured,
                Asset = new Asset() { SaidKeyCode = "ABCD" }
            };

            if (addConfig)
            {
                ds.DatasetFileConfigs.Add(MockDatasetFileConfig(ds));
            }

            return ds;
        }
        public static MigrationHistory MockMigrationHistory_DEV_to_TEST()
        {
            MigrationHistory migrationHistory = new MigrationHistory() 
            { 
                MigrationHistoryId = 1,
                SourceDatasetId = 1000,
                SourceNamedEnvironment = "DEV",
                TargetDatasetId = 1001,
                TargetNamedEnvironment = "TEST"
            };
            return migrationHistory;
        }

        public static MigrationHistory MockMigrationHistory_TEST_to_DEV()
        {
            MigrationHistory migrationHistory = new MigrationHistory()
            {
                MigrationHistoryId = 2,
                SourceDatasetId = 1001,
                SourceNamedEnvironment = "TEST",
                TargetDatasetId = 1000,
                TargetNamedEnvironment = "DEV"
            };
            return migrationHistory;
        }

        public static MigrationHistory MockMigrationHistory_TEST_to_QUAL()
        {
            MigrationHistory migrationHistory = new MigrationHistory()
            {
                MigrationHistoryId = 3,
                SourceDatasetId = 1001,
                SourceNamedEnvironment = "TEST",
                TargetDatasetId = 1002,
                TargetNamedEnvironment = "QUAL"
            };
            return migrationHistory;
        }

        public static DatasetRelativeDto MockRelativeDto_DEV()
        {
            DatasetRelativeDto relative = new DatasetRelativeDto(1000, "DEV", String.Empty);
            return relative;
        }

        public static DatasetRelativeDto MockRelativeDto_TEST()
        {
            DatasetRelativeDto relative = new DatasetRelativeDto(1001, "TEST", String.Empty);
            return relative;
        }

        public static DatasetRelativeDto MockRelativeDto_QUAL()
        {
            DatasetRelativeDto relative = new DatasetRelativeDto(1002, "QUAL", String.Empty);
            return relative;
        }

        public static DatasetRelativeDto MockRelativeDto_PROD()
        {
            DatasetRelativeDto relative = new DatasetRelativeDto(1003, "PROD", String.Empty);
            return relative;
        }

        public static List<DatasetRelativeDto> MockRelativeDtos()
        {
            List<DatasetRelativeDto> relatives = new List<DatasetRelativeDto>();
            relatives.Add(MockRelativeDto_DEV());
            relatives.Add(MockRelativeDto_TEST());
            relatives.Add(MockRelativeDto_QUAL());
            relatives.Add(MockRelativeDto_PROD());
            return relatives;
        }


        public static List<DatasetDto> MockDatasetDto(List<Dataset> dsList)
        {
            List<DatasetDto> dsDtoList = new List<DatasetDto>();
            foreach (Dataset dsItem in dsList)
            {
                DatasetDto dsItemDto = new DatasetDto()
                {
                    DatasetId = dsItem.DatasetId,
                    DatasetName = dsItem.DatasetName,
                    DatasetDesc = dsItem.DatasetDesc,
                    SAIDAssetKeyCode = "ABCD"
                };
                dsDtoList.Add(dsItemDto);
            }
            return dsDtoList;
        }

        public static List<DatasetSchemaDto> MockDatasetSchemaDto(List<Dataset> dsList)
        {
            List<DatasetSchemaDto> dtoList = new List<DatasetSchemaDto>();
            foreach (Dataset ds in dsList)
            {
                DatasetSchemaDto dto = new DatasetSchemaDto()
                {
                    DatasetId = ds.DatasetId,
                    DatasetDesc = ds.DatasetDesc
                };
                dtoList.Add(dto);
            }

            return dtoList;
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

        public static DatasetFileConfig MockDatasetFileConfig(Dataset ds = null, FileSchema schema = null)
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
                Schema = schema,
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue
            };

            return dfc;
        }

        public static List<DatasetFileConfigDto> MockDatasetFileConfigDtoList(List<DatasetFileConfig> dfcList)
        {
            List<DatasetFileConfigDto> dtoList = new List<DatasetFileConfigDto>();
            foreach (DatasetFileConfig config in dfcList)
            {
                DatasetFileConfigDto dto = new DatasetFileConfigDto()
                {
                    ConfigId = config.ConfigId,
                    Description = config.Description,
                    Schema = MockFileSchemaDto(config.Schema),
                    ParentDatasetId = config.ParentDataset.DatasetId
                };

                dtoList.Add(dto);
            }

            return dtoList;
        }

        public static FileSchema MockFileSchema()
        {
            FileSchema fileSchema = new FileSchema()
            {
                SchemaId = 23,
                Name = "Test_Schema_23",
                StorageCode = "199992"
            };

            return fileSchema;
        }

        public static SchemaRevision MockSchemaRevision()
        {
            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Id = 57,
                SchemaRevision_Name = "Test Revision"
            };

            return revision;
        }

        public static FileSchemaDto MockFileSchemaDto(Schema scm)
        {
            FileSchemaDto dto = new FileSchemaDto()
            {
                SchemaId = scm.SchemaId,
                Name = scm.Name
            };

            return dto;
        }

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
                Name = "CSV",
                Created = System.DateTime.Now.AddYears(-13),
                CreatedUser = "072984"
            };

            List<FileExtension> feList = new List<FileExtension>();

            feList.Add(fe);

            return feList;
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


        public static DatasetFile MockDatasetFile(Dataset ds, DatasetFileConfig dfc, IApplicationUser user)
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
                IsBundled = false,
                ParentDatasetFileId = 23,
                VersionId = "QWENUD-asdf9320123n90afs",
                Information = "Information Text",
                Size = 1234567890,
                FlowExecutionGuid = "20211209133645",
                RunInstanceGuid = "20211210143750",
                FileExtension = "csv",
                Schema = MockFileSchema(),
                SchemaRevision = MockSchemaRevision(),
                FileKey = "test/key/file.txt",
                FileBucket = "test-bucket-name",
                ETag = "etag-string-value",
                OriginalFileName = "zzztest0614.csv",
                ObjectStatus = ObjectStatusEnum.Active
            };

            return df;
        }

        public static DatasetFile MockDatasetFile(Dataset ds, DatasetFileConfig dfc, IApplicationUser user, ObjectStatusEnum objectStatus)
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
                IsBundled = false,
                ParentDatasetFileId = 23,
                VersionId = "QWENUD-asdf9320123n90afs",
                Information = "Information Text",
                Size = 1234567890,
                FlowExecutionGuid = "20211209133645",
                RunInstanceGuid = "20211210143750",
                FileExtension = "csv",
                Schema = MockFileSchema(),
                SchemaRevision = MockSchemaRevision(),
                FileKey = "test/key/file.txt",
                FileBucket = "test-bucket-name",
                ETag = "etag-string-value",
                OriginalFileName = "zzztest0614.csv",
                ObjectStatus = objectStatus
            };

            return df;
        }


        public static DatasetFile MockDatasetFileB(Dataset ds, DatasetFileConfig dfc, IApplicationUser user)
        {
            DatasetFile df = new DatasetFile()
            {
                DatasetFileId = 4000,
                FileName = "2014.annual.singlefile.csv",
                Dataset = ds,
                UploadUserName = user.AssociateId,
                CreatedDTM = System.DateTime.Now.AddYears(-12),
                ModifiedDTM = System.DateTime.Now.AddYears(-12),
                FileLocation = "data-dev/government/quarterly_census_of_employment_and_wages/235/2018/1/18/2014.annual.singlefile.csv",
                DatasetFileConfig = dfc,
                IsBundled = false,
                ParentDatasetFileId = 23,
                VersionId = "QWENUD-asdf9320123n90afs",
                Information = "Information Text",
                Size = 1234567890,
                FlowExecutionGuid = "20211209133645",
                RunInstanceGuid = "20211210143750",
                FileExtension = "csv",
                Schema = MockFileSchema(),
                SchemaRevision = MockSchemaRevision(),
                FileKey = "test/key/file.txt",
                FileBucket = "test-bucket-name",
                ETag = "etag-string-value",
                OriginalFileName = "b",
                ObjectStatus = ObjectStatusEnum.Active
            };

            return df;
        }


        public static DatasetFile MockDatasetFileC(Dataset ds, DatasetFileConfig dfc, IApplicationUser user)
        {
            DatasetFile df = new DatasetFile()
            {
                DatasetFileId = 5000,
                FileName = "2014.annual.singlefile.csv",
                Dataset = ds,
                UploadUserName = user.AssociateId,
                CreatedDTM = System.DateTime.Now.AddYears(-12),
                ModifiedDTM = System.DateTime.Now.AddYears(-12),
                FileLocation = "data-dev/government/quarterly_census_of_employment_and_wages/235/2018/1/18/2014.annual.singlefile.csv",
                DatasetFileConfig = dfc,
                IsBundled = false,
                ParentDatasetFileId = 23,
                VersionId = "QWENUD-asdf9320123n90afs",
                Information = "Information Text",
                Size = 1234567890,
                FlowExecutionGuid = "20211209133645",
                RunInstanceGuid = "20211210143750",
                FileExtension = "csv",
                Schema = MockFileSchema(),
                SchemaRevision = MockSchemaRevision(),
                FileKey = "test/key/file.txt",
                FileBucket = "test-bucket-name",
                ETag = "etag-string-value",
                OriginalFileName = "c",
                ObjectStatus = ObjectStatusEnum.Active
            };

            return df;
        }

        public static DatasetFileDto MockDatasetFileDto(IApplicationUser user = null)
        {
            DatasetFileDto dto = new DatasetFileDto()
            {
                DatasetFileId = 3000,
                FileName = "dto.txt",
                Dataset = 1000,
                UploadUserName = (user == null) ? "012345" : user.AssociateId,
                CreateDTM = System.DateTime.Now.AddYears(-12),
                ModifiedDTM = System.DateTime.Now.AddYears(-12),
                FileLocation = "data-dev/government/quarterly_census_of_employment_and_wages/235/2018/1/18/2014.annual.singlefile.csv",
                Schema = 23,
                SchemaRevision = 57
            };

            return dto;
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

                DatasetConfig = dfc != null ? dfc : MockDatasetFileConfig(null),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                IsGeneric = true,
                ObjectStatus = ObjectStatusEnum.Active,

                JobOptions = rjo
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

            if (user != null)
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

            EventType et2 = new EventType();
            et2.Description = "Searched";
            et2.Severity = 1;
            et2.Display = false;
            et2.Type_ID = 1;

            EventType et3 = new EventType();
            et3.Description = "Created File";
            et3.Severity = 1;
            et3.Display = true;
            et3.Type_ID = 1;

            List<EventType> types = new List<EventType>();
            types.Add(et);
            types.Add(et2);
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
            subscription.EventType = MockEventTypes()[2];
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

        public static List<ProducerS3DropAction> MockProducerS3DropActions()
        {
            const string prefix = "producers3drop/";
            const string name = "Producer S3 Drop";
            var actions = new[] {new ProducerS3DropAction()
            {
                Id = 12,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ProducerS3DropAction(){
                Id = 15,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ProducerS3DropAction(){
                Id = 35,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ProducerS3DropAction(){
                Id = 20,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ProducerS3DropAction(){
                Id = 40,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };

            return actions.ToList();
        }

        public static List<RawStorageAction> MockRawStorageActions()
        {
            const string prefix = "raw/";
            const string name = "Raw Storage";
            var actions = new[] {new RawStorageAction()
            {
                Id = 2,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new RawStorageAction(){
                Id = 22,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new RawStorageAction(){
                Id = 42,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new RawStorageAction(){
                Id = 16,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new RawStorageAction(){
                Id = 36,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<QueryStorageAction> MockQueryStorageActions()
        {
            const string prefix = "rawquery/";
            const string name = "Query Storage";
            var actions = new[] {new QueryStorageAction()
            {
                Id = 3,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new QueryStorageAction(){
                Id = 23,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new QueryStorageAction(){
                Id = 43,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new QueryStorageAction(){
                Id = 17,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new QueryStorageAction(){
                Id = 37,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<ConvertToParquetAction> MockConvertToParquetActions()
        {
            const string prefix = "parquet/";
            const string name = "ConvertToParquet";
            var actions = new[] {new ConvertToParquetAction()
            {
                Id = 6,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ConvertToParquetAction(){
                Id = 24,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ConvertToParquetAction(){
                Id = 44,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ConvertToParquetAction(){
                Id = 39,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ConvertToParquetAction(){
                Id = 19,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<UncompressZipAction> MockUncompressZipActions()
        {
            const string prefix = "uncompresszip/";
            const string name = "Uncompress Zip";
            var actions = new[] {new UncompressZipAction()
            {
                Id = 5,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new UncompressZipAction(){
                Id = 25,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new UncompressZipAction(){
                Id = 45,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<GoogleApiAction> MockGoogleApiActions()
        {
            const string prefix = "googleapipreprocessing/";
            const string name = "Google Api";
            var actions = new[] {new GoogleApiAction()
            {
                Id = 8,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new GoogleApiAction(){
                Id = 26,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new GoogleApiAction(){
                Id = 46,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<ClaimIQAction> MockClaimIQActions()
        {
            const string prefix = "claimiqpreprocessing/";
            const string name = "ClaimIQ";
            var actions = new[] {new ClaimIQAction()
            {
                Id = 9,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ClaimIQAction(){
                Id = 27,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new ClaimIQAction(){
                Id = 47,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<UncompressGzipAction> MockUncompressGzipActions()
        {
            const string prefix = "uncompressgzip/";
            const string name = "Uncompress Gzip";
            var actions = new[] {new UncompressGzipAction()
            {
                Id = 10,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new UncompressGzipAction(){
                Id = 28,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new UncompressGzipAction(){
                Id = 48,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<FixedWidthAction> MockFixedWidthActions()
        {
            const string prefix = "fixedwidthpreprocessing/";
            const string name = "Fixed Width";
            var actions = new[] {new FixedWidthAction()
            {
                Id = 11,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new FixedWidthAction(){
                Id = 29,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new FixedWidthAction(){
                Id = 49,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<XMLAction> MockXmlActions()
        {
            const string prefix = "xmlpreprocessing/";
            const string name = "XML";
            var actions = new[] {new XMLAction()
            {
                Id = 21,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },new XMLAction()
            {
                Id = 41,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },new XMLAction()
            {
                Id = 13,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new XMLAction(){
                Id = 30,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new XMLAction(){
                Id = 50,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<JsonFlatteningAction> MockJsonFlatteningActions()
        {
            const string prefix = "jsonflattening/";
            const string name = "JSON Flattening";
            var actions = new[] {new JsonFlatteningAction()
            {
                Id = 14,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new JsonFlatteningAction(){
                Id = 31,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new JsonFlatteningAction(){
                Id = 51,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
             } };
            return actions.ToList();
        }

        public static List<SchemaLoadAction> MockSchemaLoadActions()
        {
            const string prefix = "schemaload/";
            const string name = "Schema Load";
            var actions = new[] {new SchemaLoadAction()
            {
                Id = 18,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },new SchemaLoadAction()
            {
                Id = 38,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },new SchemaLoadAction()
            {
                Id = 4,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new SchemaLoadAction(){
                Id = 32,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            },
            new SchemaLoadAction(){
                Id = 52,
                Name = name,
                TargetStorageBucket = DlstDatasetBucketNonProd,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<SchemaMapAction> MockSchemaMapActions()
        {
            const string prefix = "schemamap/";
            const string name = "Schema Map";
            var actions = new[] {new SchemaMapAction()
            {
                Id = 7,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static List<S3DropAction> MockS3DropActions()
        {
            const string prefix = "s3drop/";
            const string name = "S3 Drop";
            var actions = new[] {new S3DropAction()
            {
                Id = 1,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix,
                TriggerPrefix = prefix
            } };
            return actions.ToList();
        }

        public static DataFlow MockDataFlow()
        {
            DataFlow df = new DataFlow()
            {
                Id = 90,
                Name = "MockDataFlow",
                SaidKeyCode = "DATA",
                NamedEnvironment = "TEST",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue,
                IngestionType = (int)IngestionType.DFS_Drop
            };
            return df;
        }

        public static DataFlow MockDataFlowTopic()
        {
            DataFlow df = new DataFlow()
            {
                Id = 90,
                Name = "MockDataFlow",
                SaidKeyCode = "DATA",
                NamedEnvironment = "TEST",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue,
                TopicName = "TestTopic"
            };
            return df;
        }

        public static DataFlow MockDataFlowIsBackFilledNo()
        {
            DataFlow df = new DataFlow()
            {
                Id = 77,
                Name = "Leonardo",
                SaidKeyCode = "DATA",
                NamedEnvironment = "SUPERPROD",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue,
                IngestionType = (int) GlobalEnums.IngestionType.Topic,
                IsBackFillRequired = false,
            };
            return df;
        }

        public static DataFlow MockDataFlowIsBackFilledYes()
        {
            DataFlow df = new DataFlow()
            {
                Id = 77,
                Name = "Leonardo",
                SaidKeyCode = "DATA",
                NamedEnvironment = "SUPERPROD",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue,
                IngestionType = (int)GlobalEnums.IngestionType.Topic,
                IsBackFillRequired = true,
            };
            return df;
        }

        public static Security MockSecurity(IList<string> permissions)
        {
            var security = new Security()
            {
                Tickets = new List<SecurityTicket>()
            };

            foreach (var perm in permissions)
            {
                security.Tickets.Add(
                    new SecurityTicket()
                    {
                        IsAddingPermission = true,
                        AddedPermissions = new List<SecurityPermission>()
                        {
                            new SecurityPermission()
                            {
                                IsEnabled = true,
                                Permission = new Permission() {
                                    PermissionCode = perm
                                }
                            }
                        }
                    }
                );
            }
            return security;
        }

        public static DataFlowDto MockDataFlowDto(DataFlow flow, SchemaMapDto schemaMapDto)
        {
            var flowDto = new DataFlowDto()
            {
                Id = (flow == null) ? 0 : flow.Id,
                SchemaMap = new List<SchemaMapDto>() { schemaMapDto },
                FlowGuid = Guid.Empty,
                SaidKeyCode = "DATA",
                DatasetId = 1,
                SchemaId = 23,
                Name = "TheFlowTest",
                CreateDTM = DateTime.Now,
                CreatedBy = "072984",
                DFQuestionnaire = null,
                IngestionType = (int) IngestionType.DFS_Drop,
                RetrieverJob = null,
                IsCompressed = false,
                IsBackFillRequired = true,
                CompressionType = 0,
                CompressionJob = null,
                IsPreProcessingRequired = false,
                PreProcessingOption = 0,
                FlowStorageCode = "",
                AssociatedJobs = null,
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue,
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                PrimaryContactId = "072984",
                IsSecured = true,
                Security = null
            };

            return flowDto;
        }

        public static DataFlowDto MockDataFlowDtoTopic(DataFlow flow, SchemaMapDto schemaMapDto)
        {
            var flowDto = new DataFlowDto()
            {
                Id = (flow == null) ? 0 : flow.Id,
                SchemaMap = new List<SchemaMapDto>() { schemaMapDto },
                FlowGuid = Guid.Empty,
                SaidKeyCode = "DATA",
                DatasetId = 1,
                SchemaId = 23,
                Name = "TopicFlowTest",
                CreateDTM = DateTime.Now,
                CreatedBy = "072984",
                DFQuestionnaire = null,
                IngestionType = (int) IngestionType.Topic,
                RetrieverJob = null,
                IsCompressed = false,
                IsBackFillRequired = true,
                CompressionType = 0,
                CompressionJob = null,
                IsPreProcessingRequired = false,
                PreProcessingOption = 0,
                FlowStorageCode = "",
                AssociatedJobs = null,
                ObjectStatus = ObjectStatusEnum.Active,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue,
                NamedEnvironment = "DEV",
                NamedEnvironmentType = NamedEnvironmentType.NonProd,
                PrimaryContactId = "072984",
                IsSecured = true,
                Security = null,
                TopicName = "Topic-Name-Test"
            };

            return flowDto;
        }

        public static ConnectorCreateRequestDto MockConnectorCreateRequestDto()
        {
            ConnectorCreateRequestConfigDto config = new ConnectorCreateRequestConfigDto()
            {
                ConnectorClass = "io.confluent.connect.s3.S3SinkConnector",
                S3Region = "us-east-2",
                TopicsDir = "topics_2",
                FlushSize = "1",
                TasksMax = "1",
                Timezone = "UTC",
                Transforms = "InsertMetadata",
                Locale = "en-US",
                S3PathStyleAccessEnabled = "false",
                FormatClass = "io.confluent.connect.s3.format.json.JsonFormat",
                S3AclCanned = "bucket-owner-full-control",
                TransformsInsertMetadataPartitionField = "kafka_partition",
                ValueConverter = "org.apache.kafka.connect.json.JsonConverter",
                S3ProxyPassword = "nopass",
                KeyConverter = "org.apache.kafka.connect.converters.ByteArrayConverter",
                S3BucketName = "sentry-dlst-qual-droplocation-ae2",
                PartitionDurationMs = "86400000",
                S3ProxyUser = "SV_DATA_S3CON_I_Q_V1",
                S3SseaName = "AES256",
                TransformsInsertMetadataOffsetField = "kafka_offset",
                FileDelim = "_",
                Topics = "Gojira",
                PartitionerClass = "io.confluent.connect.storage.partitioner.TimeBasedPartitioner",
                ValueConverterSchemasEnable = "false",
                TransformsInsertMetadataTimestampField = "kafka_timestamp",
                Name = "S3_Gojira_001",
                StorageClass = "io.confluent.connect.s3.storage.S3Storage",
                RotateScheduleIntervalMs = "86400000",
                PathFormat = "YYYY/MM/dd",
                TimestampExtractor = "Record",
                TransformsInsertMetadataType = "org.apache.kafka.connect.transforms.InsertField$Value",
                S3ProxyUrl = "https://app-proxy-nonprod.sentry.com:8080"
            };

            ConnectorCreateRequestDto request = new ConnectorCreateRequestDto()
            {
                Name = "S3_Gojira_001",
                Config = config
            };

            return request;
        }


        public static DatasetMigrationRequest MockRequestMontana()
        {
            DatasetMigrationRequest request = new DatasetMigrationRequest()
            {
                SourceDatasetId = 3,
                TargetDatasetId = 53,
                TargetDatasetNamedEnvironment = "NRDEV"
            };

            return request;
        }


        public static SchemaMigrationRequestResponse MockSchemaResponseGlacier()
        {
            SchemaMigrationRequestResponse schemaResponse = new SchemaMigrationRequestResponse()
            {
                DataFlowMigrationReason = "Target dataflow metadata already exists",
                DataFlowName =  "Glacier",
                MigratedDataFlow =   true ,
                MigratedSchema  = true,
                MigratedSchemaRevision = true,
                SchemaMigrationReason =  "Schema configuration existed in target",
                SchemaName = "Glacier",
                SchemaRevisionMigrationReason  = "No column metadata on source schema",
                SchemaRevisionName = "GlacierRevisionName",
                SourceSchemaId  = 1,
                TargetDataFlowId  =  51,
                TargetSchemaId = 51,
                TargetSchemaRevisionId = 51
            };
            return schemaResponse;
        }

        public static SchemaMigrationRequestResponse MockSchemaResponseGreatFalls()
        {
            SchemaMigrationRequestResponse schemaResponse = new SchemaMigrationRequestResponse()
            {
                DataFlowMigrationReason = "Target dataflow metadata already exists",
                DataFlowName = "GreatFalls",
                MigratedDataFlow = true,
                MigratedSchema = true,
                MigratedSchemaRevision = true,
                SchemaMigrationReason = "Schema configuration existed in target",
                SchemaName = "GreatFalls",
                SchemaRevisionMigrationReason = "No column metadata on source schema",
                SchemaRevisionName = "GreatFallsRevisionName",
                SourceSchemaId = 2,
                TargetDataFlowId = 52,
                TargetSchemaId = 52,
                TargetSchemaRevisionId = 52
            };
            return schemaResponse;
        }

        public static SchemaMigrationRequestResponse MockSchemaResponseNewYork()
        {
            SchemaMigrationRequestResponse schemaResponse = new SchemaMigrationRequestResponse()
            {
                DataFlowMigrationReason = "Target dataflow metadata already exists",
                MigratedDataFlow = false,
                MigratedSchema = false,
                MigratedSchemaRevision = false,
                SchemaMigrationReason = "Schema configuration existed in target",
                SourceSchemaId = 7
                
            };
            return schemaResponse;
        }

        public static DatasetMigrationRequestResponse MockResponseMontana()
        {
            List<SchemaMigrationRequestResponse> schemaResponses = new List<SchemaMigrationRequestResponse>();
            schemaResponses.Add(MockClasses.MockSchemaResponseGlacier());
            schemaResponses.Add(MockClasses.MockSchemaResponseGreatFalls());
            schemaResponses.Add(MockClasses.MockSchemaResponseNewYork());

            DatasetMigrationRequestResponse response = new DatasetMigrationRequestResponse()
            {
                DatasetId = 53,
                DatasetMigrationReason = "Dataset already exists in target named environment",
                DatasetName = "Montana",
                IsDatasetMigrated = true,
                SchemaMigrationResponses = schemaResponses
            };
            return response;
        }

        public static MigrationHistory MockHistoryMontana()
        {
            MigrationHistory history = new MigrationHistory()
            {
                CreateDateTime = DateTime.Now,
                MigrationHistoryId	= 0,
                SourceDatasetId = 3,
                SourceNamedEnvironment = "DEV",
                TargetDatasetId = 53,
                TargetNamedEnvironment	= "NRDEV"
            };
            return history;
        }


        public static MigrationHistoryDetail MockHistoryDetailDataset()
        {

            MigrationHistoryDetail historyDetail = new MigrationHistoryDetail()
            {

                SourceDatasetId = 3,
                DatasetId = 53,
                DatasetMigrationMessage =  "Dataset already exists in target named environment",
                DatasetName = "Montana",
                MigrationHistoryId = 0,
                DataFlowId = null,
                SchemaId = null,
                SchemaRevisionId = null
            };
            return historyDetail;
        }


        public static MigrationHistoryDetail MockHistoryDetailSchemaGlacier()
        {

            MigrationHistoryDetail historyDetail = new MigrationHistoryDetail()
            {

                SourceDatasetId = 3,
                DatasetId = null,
                SchemaName = "Glacier",
                MigrationHistoryId = 0,
                DataFlowId = null,
                SourceSchemaId = 1,
                SchemaId = 51,
                SchemaMigrationMessage = "Schema configuration existed in target",
                SchemaRevisionId = 51,
                SchemaRevisionName = "GlacierRevisionName"

            };
            return historyDetail;
        }


        public static MigrationHistoryDetail MockHistoryDetailSchemaGreatFalls()
        {

            MigrationHistoryDetail historyDetail = new MigrationHistoryDetail()
            {

                SourceDatasetId = 3,
                DatasetId = null,
                SchemaName = "GreatFalls",
                MigrationHistoryId = 0,
                DataFlowId = null,
                SourceSchemaId = 2,
                SchemaId = 52,
                SchemaMigrationMessage = "Schema configuration existed in target",
                SchemaRevisionId = 52,
                SchemaRevisionName = "GreatFallsRevisionName"

            };
            return historyDetail;
        }

        public static MigrationHistoryDetail MockHistoryDetailSchemaNewYork()
        {

            MigrationHistoryDetail historyDetail = new MigrationHistoryDetail()
            {

                SourceDatasetId = 3,
                DatasetId = null,
                SchemaName = "NewYork",
                MigrationHistoryId = 0,
                DataFlowId = null,
                SourceSchemaId = 7,
                SchemaId = null,
                SchemaMigrationMessage = "Schema configuration existed in target",
                SchemaRevisionId = null,
                SchemaRevisionName = null

            };
            return historyDetail;
        }

    }
}
