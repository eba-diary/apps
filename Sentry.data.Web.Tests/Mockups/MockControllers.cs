using Moq;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.data.Web.Controllers;
using Sentry.FeatureFlags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    public static class MockControllers
    {
        public static DataAssetController MockDataAssetController(DataAsset da, IApplicationUser user = null)
        {
            List<DataAsset> daList = new List<DataAsset> { da };

            MockRepository mr = new MockRepository(MockBehavior.Default);

            var mockDatasetContext = mr.Create<IDatasetContext>();
            var mockAssociateService = mr.Create<IAssociateInfoProvider>();
            var mockMetadataRepositoryProvider = mr.Create<IMetadataRepositoryProvider>();
            var mockDataAssetContext = mr.Create<IDataAssetContext>();

            var mockMetadataRepositoryService = new MetadataRepositoryService(mockMetadataRepositoryProvider.Object);
            var mockUserService = mr.Create<IUserService>();

            mockDataAssetContext.Setup(x => x.GetDataAsset(da.Name)).Returns(da);
            mockDataAssetContext.Setup(x => x.GetDataAsset(da.Id)).Returns(da);
            mockDataAssetContext.Setup(x => x.GetDataAssets()).Returns(daList);
            mockDataAssetContext.Setup(x => x.GetAssetNotificationsByDataAssetId(da.Id)).Returns(da.AssetNotifications);

            mockDatasetContext.Setup(x => x.EventTypes).Returns(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Setup(x => x.EventStatus).Returns(MockClasses.MockEventStatuses().AsQueryable());

            mockDataAssetContext.Setup(x => x.GetAssetNotificationByID(da.AssetNotifications[0].NotificationId)).Returns(da.AssetNotifications[0]);

            mockUserService.Setup(x => x.GetCurrentUser()).Returns(user ?? MockUsers.App_DataMgmt_Admin_User().Object);

            var dac = new DataAssetController(mockMetadataRepositoryService, mockDataAssetContext.Object, mockDatasetContext.Object, mockAssociateService.Object, mockUserService.Object);
            dac.SharedContext = new SharedContextModel { CurrentUser = user };

            return dac;
        }

        public static DatasetController MockDatasetController(Dataset ds, IApplicationUser user, List<DatasetSubscription> datasetSubscriptions = null)
        {
            Random r = new Random();
            MockRepository mr = new MockRepository(MockBehavior.Default);

            var mockDatasetContext = mr.Create<IDatasetContext>();
            var mockAssociateService = mr.Create<IAssociateInfoProvider>();

            var mockObsidianService = mr.Create<IObsidianService>();
            var mockS3Provider = mr.Create<S3ServiceProvider>();

            var mockUserService = mr.Create<IUserService>();

            var mockDatasetService = mr.Create<IDatasetService>();
            var mockConfigService = mr.Create<IConfigService>();
            
            var mockEventService = mr.Create<IEventService>();

            var mockFeatureFlag = new Mock<IDataFeatures>();

            mockUserService.Setup(x => x.GetCurrentUser()).Returns(user ?? MockUsers.App_DataMgmt_Admin_User().Object);

            mockAssociateService.Setup(x => x.GetAssociateInfo(user.AssociateId)).Returns(new Associates.Associate() { FullName = "Bill Nye" });

            if (ds != null)
            {
                List<Dataset> dsList = new List<Dataset> { ds };

                mockDatasetContext.Setup(x => x.Datasets).Returns(dsList.AsQueryable());
                mockDatasetContext.Setup(x => x.GetDatasetCount()).Returns(dsList.Count);
                mockDatasetContext.Setup(x => x.GetById(ds.DatasetId)).Returns(ds);
                mockDatasetContext.Setup(x => x.GetAllUserSubscriptionsForDataset(user.AssociateId, ds.DatasetId)).Returns(datasetSubscriptions ?? new List<DatasetSubscription>());
            }

            mockDatasetContext.Setup(x => x.Merge<Dataset>(ds)).Returns(ds);
            mockDatasetContext.Setup(x => x.Events).Returns(new List<Event>().AsQueryable());
            mockDatasetContext.Setup(x => x.EventTypes).Returns(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Setup(x => x.GetAllIntervals()).Returns(MockClasses.MockIntervals());
            mockDatasetContext.Setup(x => x.GetInterval("Never")).Returns(MockClasses.MockIntervals().FirstOrDefault(x =>x.Description == "Never"));
            mockDatasetContext.Setup(x => x.EventStatus).Returns(MockClasses.MockEventStatuses().AsQueryable());
            mockDatasetContext.Setup(x => x.FileExtensions).Returns(MockClasses.MockFileExtensions().AsQueryable());
            mockDatasetContext.Setup(x => x.DataSources).Returns(MockClasses.MockDataSources().AsQueryable());
            mockDatasetContext.Setup(x => x.Categories).Returns(MockClasses.MockCategories().AsQueryable());
            mockDatasetContext.Setup(x => x.GetCategoryById(0)).Returns(MockClasses.MockCategories()[0]);
            mockDatasetContext.Setup(x => x.GetAllDatasetScopeTypes()).Returns(MockClasses.MockScopeTypes());
            mockDatasetContext.Setup(x => x.isDatasetNameDuplicate(ds.DatasetName, ds.DatasetCategories.First().Name)).Returns(false);
            mockDatasetContext.Setup(x => x.GetNextStorageCDE()).Returns(r.Next(0, 1000000));

            mockUserService.Setup(x => x.GetCurrentUser()).Returns(user);

            var updateSearchPagesFeature = new Mock<IFeatureFlag<bool>>();
            updateSearchPagesFeature.Setup(x => x.GetValue()).Returns(false);
            mockFeatureFlag.SetupGet(x => x.CLA3756_UpdateSearchPages).Returns(updateSearchPagesFeature.Object);

            var exposeDataFlowFeature = new Mock<IFeatureFlag<bool>>();
            exposeDataFlowFeature.Setup(x => x.GetValue()).Returns(false);
            mockFeatureFlag.SetupGet(x => x.Expose_Dataflow_Metadata_CLA_2146).Returns(exposeDataFlowFeature.Object);

            var dsc = new DatasetController(mockDatasetContext.Object, mockS3Provider.Object, mockUserService.Object, 
                mockAssociateService.Object, mockObsidianService.Object, mockDatasetService.Object, mockEventService.Object, mockConfigService.Object,
                mockFeatureFlag.Object, null, null, null, null, null, null, null);
            dsc.SharedContext = new SharedContextModel { CurrentUser = user };

            return dsc;
        }

        public static ConfigController MockConfigController(DatasetFileConfig dfc, IApplicationUser user)
        {
            MockRepository mr = new MockRepository(MockBehavior.Default);
            var mockDatasetContext = mr.Create<IDatasetContext>();
            var mockAssociateService = mr.Create<IAssociateInfoProvider>();

            var mockConfigService = mr.Create<IConfigService>();
            var mockEvensService = mr.Create<IEventService>();
            var mockDataFeatures = mr.Create<IDataFeatures>();
            mockDataFeatures.Setup(x => x.CLA4925_ParquetFileType.GetValue()).Returns(true);

            var mockUserService = mr.Create<IUserService>();
            var mockDatasetService = mr.Create<IDatasetService>();
            var mockObsidianService = mr.Create<IObsidianService>();
            var mockSecurityService = mr.Create<ISecurityService>();
            var mockSchemaService = mr.Create<ISchemaService>();

            var ds = dfc.ParentDataset;

            if (ds != null)
            {
                mockDatasetContext.Setup(x => x.GetById(ds.DatasetId)).Returns(ds);
                mockDatasetContext.Setup(x => x.GetById<Dataset>(ds.DatasetId)).Returns(ds);
                mockDatasetContext.Setup(x => x.getDatasetFileConfigs(dfc.ConfigId)).Returns(dfc);
                mockDatasetContext.Setup(x => x.GetAllDatasetScopeTypes()).Returns(new List<DatasetScopeType>());
            }

            mockDatasetContext.Setup(x => x.EventTypes).Returns(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Setup(x => x.EventStatus).Returns(MockClasses.MockEventStatuses().AsQueryable());
            mockDatasetContext.Setup(x => x.FileExtensions).Returns(MockClasses.MockFileExtensions().AsQueryable());

            mockUserService.Setup(x => x.GetCurrentUser()).Returns(user ?? MockUsers.App_DataMgmt_Admin_User().Object);

            var cc = new ConfigController(mockDatasetContext.Object, mockUserService.Object, mockAssociateService.Object, mockConfigService.Object, mockEvensService.Object, 
                mockDatasetService.Object, mockObsidianService.Object, mockSecurityService.Object, mockSchemaService.Object, mockDataFeatures.Object, null, null);
            cc.SharedContext = new SharedContextModel { CurrentUser = user };

            return cc;
        }
    }
}
