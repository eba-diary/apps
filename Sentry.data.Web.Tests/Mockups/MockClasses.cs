﻿using Sentry.data.Core;
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
        public static Dataset MockDataset(IApplicationUser user = null, Boolean addConfig = false)
        {
            Dataset ds = new Dataset()
            {
                DatasetId = 1000,
                Category = "Claim",
                DatasetCategory = MockCategories()[0],
                DatasetName = "Claim Dataset",
                DatasetDesc = "Test Claim Dataset",
                DatasetInformation = "Specific Information regarding datasetfile consumption",
                CreationUserName = user != null ? user.DisplayName : "Nye, Bill",
                SentryOwnerName = user != null ? user.AssociateId : "012345",
                UploadUserName = user != null ? user.DisplayName : "Nye, Bill",
                OriginationCode = "Internal",
                DatasetDtm = System.DateTime.Now.AddYears(-13),
                ChangedDtm = System.DateTime.Now.AddYears(-12),
                S3Key = "data-dev/government/quarterly_census_of_employment_and_wages/",
                IsSensitive = false,
                CanDisplay = true,
                DatasetFiles = new List<DatasetFile>(),
                DatasetFileConfigs = new List<DatasetFileConfig>()
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
                IsGeneric = true,
                ParentDataset = ds != null ? ds : MockDataset(),
                DatasetScopeType = MockScopeTypes()[0],
                FileExtension = MockFileExtensions()[0],
                RetrieverJobs = new List<RetrieverJob>()
            };

            return dfc;
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
            dfs.Description = "Basic Description";

            dataSources.Add(dfs);

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
                CreateDTM = System.DateTime.Now.AddYears(-12),
                ModifiedDTM = System.DateTime.Now.AddYears(-12),
                FileLocation = "data-dev/government/quarterly_census_of_employment_and_wages/235/2018/1/18/2014.annual.singlefile.csv",
                DatasetFileConfig = dfc,
                IsBundled = false,
                IsUsable = true            
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
                OverwriteDataFile = true,
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

                DatasetConfig = dfc != null? dfc : MockDataFileConfig(null),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                IsGeneric = true,

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

            List<AssetNotifications> anList = new List<AssetNotifications>();
            anList.Add(GetMockAssetNotifications(da));

            da.AssetNotifications = anList;

            return da;
        }

        public static AssetNotifications GetMockAssetNotifications(DataAsset da, IApplicationUser user = null)
        {
            AssetNotifications an = new AssetNotifications();
            an.NotificationId = 1;
            an.ParentDataAsset = da;
            an.MessageSeverity = 1;
            an.Message = "Alert Message";
            an.StartTime = DateTime.Now.AddHours(-1).AddMinutes(1);
            an.ExpirationTime = DateTime.Now.AddDays(1);

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
            Event e = new Event();
            e.EventType = MockEventTypes()[0];
            e.Status = MockEventStatuses()[0];            
            e.TimeCreated = DateTime.Now;
            e.TimeNotified = DateTime.Now;
            e.IsProcessed = false;
            e.UserWhoStartedEvent = "012345";

            return e;
        }

    }
}