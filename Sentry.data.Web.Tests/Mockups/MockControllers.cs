﻿using Rhino.Mocks;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.data.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    public static class MockControllers
    {
        public static DataAssetController MockDataAssetController(DataAsset da, IApplicationUser user = null)
        {
            List<DataAsset> daList = new List<DataAsset>();
            daList.Add(da);

            var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();
            var mockAssociateService = MockRepository.GenerateStub<IAssociateInfoProvider>();
            var mockExtendedUserInfoProvider = MockRepository.GenerateStub<IExtendedUserInfoProvider>();
            var mockCurrentUserIdProvider = MockRepository.GenerateStub<ICurrentUserIdProvider>();
            var mockMetadataRepositoryProvider = MockRepository.GenerateStub<IMetadataRepositoryProvider>();
            var mockDataAssetContext = MockRepository.GenerateStub<IDataAssetContext>();
            var mockSharedContextModel = MockRepository.GenerateStub<SharedContextModel>();

            var mockMetadataRepositoryService = MockRepository.GenerateStub<MetadataRepositoryService>(mockMetadataRepositoryProvider);
            var mockUserService = MockRepository.GenerateStub<UserService>(mockDataAssetContext, mockExtendedUserInfoProvider, mockCurrentUserIdProvider);

            mockDataAssetContext.Stub(x => x.GetDataAsset(da.Name)).Return(da);
            mockDataAssetContext.Stub(x => x.GetDataAsset(da.Id)).Return(da);
            mockDataAssetContext.Stub(x => x.GetDataAssets()).Return(daList);
            mockDataAssetContext.Stub(x => x.GetAssetNotificationsByDataAssetId(da.Id)).Return(da.AssetNotifications);

            mockDatasetContext.Stub(x => x.EventTypes).Return(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Stub(x => x.EventStatus).Return(MockClasses.MockEventStatuses().AsQueryable());

            mockDataAssetContext.Stub(x => x.GetAssetNotificationByID(da.AssetNotifications[0].NotificationId)).Return(da.AssetNotifications[0]);

            mockSharedContextModel.CurrentUser = user != null ? user : MockUsers.App_DataMgmt_Admin_User();
            mockUserService.Stub(x => x.GetCurrentUser()).Return(user != null ? user : MockUsers.App_DataMgmt_Admin_User());

            var dac = new DataAssetController(mockMetadataRepositoryService, mockDataAssetContext, mockDatasetContext, mockAssociateService, mockUserService);
            dac.SharedContext = mockSharedContextModel;

            return dac;
        }

        public static DatasetController MockDatasetController(Dataset ds, IApplicationUser user, List<DatasetSubscription> datasetSubscriptions = null)
        {

            Random r = new Random();

            var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();
            var mockDataAssetContext = MockRepository.GenerateStub<IDataAssetContext>();
            var mockAssociateService = MockRepository.GenerateStub<IAssociateInfoProvider>();
            var mockExtendedUserInfoProvider = MockRepository.GenerateStub<IExtendedUserInfoProvider>();
            var mockCurrentUserIdProvider = MockRepository.GenerateStub<ICurrentUserIdProvider>();

            var mockObsidianService = MockRepository.GenerateStub<IObsidianService>();
            var mockS3Provider = MockRepository.GenerateStub<S3ServiceProvider>();
            var mockSasProvider = MockRepository.GenerateStub<ISASService>();

            var mockUserService = MockRepository.GenerateStub<UserService>(mockDataAssetContext, mockExtendedUserInfoProvider, mockCurrentUserIdProvider);
            var mockSharedContextModel = MockRepository.GenerateStub<SharedContextModel>();

            var mockDatasetService = MockRepository.GenerateStub<IDatasetService>();
            var mockConfigService = MockRepository.GenerateStub<IConfigService>();
            
            var mockEventService = MockRepository.GenerateStub<IEventService>();

            var mockFeatureFlag = MockRepository.GenerateStub<IDataFeatures>();

            mockSharedContextModel.CurrentUser = user;
            mockUserService.Stub(x => x.GetCurrentUser()).Return(user != null ? user : MockUsers.App_DataMgmt_Admin_User());

            mockAssociateService.Stub(x => x.GetAssociateInfo(user.AssociateId)).Return(new Associates.Associate() { FullName = "Bill Nye" });

            if (ds != null)
            {
                List<Dataset> dsList = new List<Dataset>();
                dsList.Add(ds);

                mockDatasetContext.Stub(x => x.Datasets).Return(dsList.AsQueryable());
                mockDatasetContext.Stub(x => x.GetDatasetCount()).Return(dsList.Count);
                mockDatasetContext.Stub(x => x.GetById(ds.DatasetId)).Return(ds);
                mockDatasetContext.Stub(x => x.IsUserSubscribedToDataset(ds.PrimaryOwnerId, ds.DatasetId)).Return(true);
                mockDatasetContext.Stub(x => x.GetAllUserSubscriptionsForDataset(user.AssociateId, ds.DatasetId)).Return(datasetSubscriptions == null ? new List<DatasetSubscription>() : datasetSubscriptions);
            }

            mockDatasetContext.Stub(x => x.Merge<Dataset>(ds)).Return(ds);
            mockDatasetContext.Stub(x => x.Events).Return(new List<Event>().AsQueryable());
            mockDatasetContext.Stub(x => x.EventTypes).Return(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Stub(x => x.GetAllIntervals()).Return(MockClasses.MockIntervals());
            mockDatasetContext.Stub(x => x.GetInterval("Never")).Return(MockClasses.MockIntervals().FirstOrDefault(x =>x.Description == "Never"));
            mockDatasetContext.Stub(x => x.EventStatus).Return(MockClasses.MockEventStatuses().AsQueryable());
            mockDatasetContext.Stub(x => x.FileExtensions).Return(MockClasses.MockFileExtensions().AsQueryable());
            mockDatasetContext.Stub(x => x.DataSources).Return(MockClasses.MockDataSources().AsQueryable());
            mockDatasetContext.Stub(x => x.Categories).Return(MockClasses.MockCategories().AsQueryable());
            mockDatasetContext.Stub(x => x.GetCategoryById(0)).Return(MockClasses.MockCategories()[0]);
            mockDatasetContext.Stub(x => x.GetAllDatasetScopeTypes()).Return(MockClasses.MockScopeTypes());
            mockDatasetContext.Stub(x => x.isDatasetNameDuplicate(ds.DatasetName, ds.DatasetCategories.First().Name)).Return(false);
            mockDatasetContext.Stub(x => x.GetNextStorageCDE()).Return(r.Next(0, 1000000));

            mockUserService.Stub(x => x.GetCurrentUser()).Return(user);

            mockFeatureFlag.Stub(x => x.Expose_Dataflow_Metadata_CLA_2146.GetValue()).Return(false);

            var dsc = new DatasetController(mockDatasetContext, mockS3Provider, mockUserService, mockSasProvider, 
                mockAssociateService, mockObsidianService, mockDatasetService, mockEventService, mockConfigService,
                mockFeatureFlag, null, null, null);
            dsc.SharedContext = mockSharedContextModel;

            return dsc;
        }

        public static ConfigController MockConfigController(DatasetFileConfig dfc, IApplicationUser user)
        {
            var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();
            var mockDataAssetContext = MockRepository.GenerateStub<IDataAssetContext>();
            var mockAssociateService = MockRepository.GenerateStub<IAssociateInfoProvider>();
            var mockExtendedUserInfoProvider = MockRepository.GenerateStub<IExtendedUserInfoProvider>();
            var mockCurrentUserIdProvider = MockRepository.GenerateStub<ICurrentUserIdProvider>();

            var mockS3Provider = MockRepository.GenerateStub<S3ServiceProvider>();
            var mockSasProvider = MockRepository.GenerateStub<ISASService>();
            var mockConfigService = MockRepository.GenerateStub<IConfigService>();
            var mockEvensService = MockRepository.GenerateStub<IEventService>();
            var mockDataFeatures = MockRepository.GenerateStub<IDataFeatures>();

            var mockUserService = MockRepository.GenerateStub<UserService>(mockDataAssetContext, mockExtendedUserInfoProvider, mockCurrentUserIdProvider);
            var mockSharedContextModel = MockRepository.GenerateStub<SharedContextModel>();
            var mockDatasetService = MockRepository.GenerateStub<IDatasetService>();
            var mockObsidianService = MockRepository.GenerateStub<IObsidianService>();
            var mockSecurityService = MockRepository.GenerateStub<ISecurityService>();
            var mockSchemaService = MockRepository.GenerateStub<ISchemaService>();

            mockSharedContextModel.CurrentUser = user;

            var ds = dfc.ParentDataset;

            mockAssociateService.Stub(x => x.GetAssociateInfo(ds.PrimaryOwnerId)).Return(new Associates.Associate() { FullName = "Bill Nye" });

            if (ds != null)
            {
                List<Dataset> dsList = new List<Dataset>();
                dsList.Add(ds);

                mockDatasetContext.Stub(x => x.GetById(ds.DatasetId)).Return(ds);
                mockDatasetContext.Stub(x => x.GetById<Dataset>(ds.DatasetId)).Return(ds);
                mockDatasetContext.Stub(x => x.getDatasetFileConfigs(dfc.ConfigId)).Return(dfc);
                mockDatasetContext.Stub(x => x.GetAllDatasetScopeTypes()).Return(new List<DatasetScopeType>());
                mockDatasetContext.Stub(x => x.IsUserSubscribedToDataset(ds.PrimaryOwnerId, ds.DatasetId)).Return(true);
                mockDatasetContext.Stub(x => x.GetAllUserSubscriptionsForDataset(ds.PrimaryOwnerId, ds.DatasetId)).Return(new List<DatasetSubscription>());
            }

            mockDatasetContext.Stub(x => x.EventTypes).Return(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Stub(x => x.EventStatus).Return(MockClasses.MockEventStatuses().AsQueryable());
            mockDatasetContext.Stub(x => x.FileExtensions).Return(MockClasses.MockFileExtensions().AsQueryable());
            //mockDatasetContext.Stub(x => x.Schemas).Return(MockClasses.MockSchemas(dfc).AsQueryable());

            mockUserService.Stub(x => x.GetCurrentUser()).Return(user != null ? user : MockUsers.App_DataMgmt_Admin_User());

            var cc = new ConfigController(mockDatasetContext, mockUserService, mockAssociateService, mockConfigService, mockEvensService, mockDatasetService, mockObsidianService, mockSecurityService, mockSchemaService, mockDataFeatures);
            cc.SharedContext = mockSharedContextModel;

            return cc;
        }

        //public static MetadataController MockMetadataController(Dataset ds, DatasetFileConfig dfc, Schema scm, IApplicationUser user)
        //{
        //    var mockConfigService = MockRepository.GenerateStub<IConfigService>();
        //    var mockDatasetService = MockRepository.GenerateStub<IDatasetService>();
        //    var mockSchemaService = MockRepository.GenerateStub<ISchemaService>();
        //    var mockDataFlowService = MockRepository.GenerateStub<IDataFlowService>();
        //    var mockHostSettings = MockRepository.GenerateStub<IHostSettings>();

        //    var mockDataAssetContext = MockRepository.GenerateStub<IDataAssetContext>();
        //    var mockExtendedUserInfoProvider = MockRepository.GenerateStub<IExtendedUserInfoProvider>();
        //    var mockCurrentUserIdProvider = MockRepository.GenerateStub<ICurrentUserIdProvider>();
        //    var mockUserService = MockRepository.GenerateStub<UserService>(mockDataAssetContext, mockExtendedUserInfoProvider, mockCurrentUserIdProvider);

        //    mockHostSettings.Stub(x => x["S3DataPrefix"]).Return("data/");


        //    mockUserService.Stub(x => x.GetCurrentUser()).Return(user != null ? user : MockUsers.App_DataMgmt_Admin_User());            


        //    var mc = new MetadataController(, mockUserService, mockConfigService, mockDatasetService, mockSchemaService, mockSecurityService, mockDataFlowService);

        //    return mc;
        //}
    }
}
