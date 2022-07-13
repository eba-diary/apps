using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Core.Tests
{
    public static class MockClasses
    {
        const string HrDatasetBucket = "sentry-dlst-dev-hrdroplocation-ae2";
        const string DlstDatasetBucket = "sentry-dlst-dev-droplocation-ae2";
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
                ds.DatasetFileConfigs.Add(MockDataFileConfig(ds));
            }

            return ds;
        }
        public static List<DatasetDto> MockDatasetDto(List<Dataset> dsList)
        {
            List<DatasetDto> dtoList = new List<DatasetDto>();
            foreach (Dataset ds in dsList)
            {
                DatasetDto dto = new DatasetDto()
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

        public static DatasetFileConfig MockDataFileConfig(Dataset ds = null, FileSchema schema = null)
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
                    Schema = MockFileSchemaDto(config.Schema)
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
                Name = "Test_Schema_23"
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
                OriginalFileName = "a",
                ObjectStatus = ObjectStatusEnum.Active
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

                DatasetConfig = dfc != null ? dfc : MockDataFileConfig(null),
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
                TargetStoragePrefix = prefix
            },
            new ProducerS3DropAction(){
                Id = 15,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
            },
            new ProducerS3DropAction(){
                Id = 20,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
            },
            new RawStorageAction(){
                Id = 22,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
            },
            new RawStorageAction(){
                Id = 16,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
            },
            new QueryStorageAction(){
                Id = 23,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
            },
            new QueryStorageAction(){
                Id = 17,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
            },
            new ConvertToParquetAction(){
                Id = 24,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
            },
            new ConvertToParquetAction(){
                Id = 19,
                Name = $"HR {name}",
                TargetStorageBucket = HrDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
            },
            new UncompressZipAction(){
                Id = 25,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
            },
            new GoogleApiAction(){
                Id = 26,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
            },
            new ClaimIQAction(){
                Id = 27,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
            },
            new UncompressGzipAction(){
                Id = 28,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
            },
            new FixedWidthAction(){
                Id = 29,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix
            },new XMLAction()
            {
                Id = 13,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix
            },
            new XMLAction(){
                Id = 30,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
            },
            new JsonFlatteningAction(){
                Id = 31,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix
            },new SchemaLoadAction()
            {
                Id = 4,
                Name = name,
                TargetStorageBucket = DataDatasetBucket,
                TargetStoragePrefix = prefix
            },
            new SchemaLoadAction(){
                Id = 32,
                Name = name,
                TargetStorageBucket = DlstDatasetBucket,
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
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
                TargetStoragePrefix = prefix
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
                DeleteIssueDTM = DateTime.MaxValue
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
                        Permissions = new List<SecurityPermission>()
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

    }
}
