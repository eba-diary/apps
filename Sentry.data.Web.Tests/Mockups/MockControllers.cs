﻿using Rhino.Mocks;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.data.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();
            var mockDataAssetContext = MockRepository.GenerateStub<IDataAssetContext>();
            var mockAssociateService = MockRepository.GenerateStub<IAssociateInfoProvider>();
            var mockExtendedUserInfoProvider = MockRepository.GenerateStub<IExtendedUserInfoProvider>();
            var mockCurrentUserIdProvider = MockRepository.GenerateStub<ICurrentUserIdProvider>();

            var mockRequestService = MockRepository.GenerateStub<IRequestContext>();

            var mockS3Provider = MockRepository.GenerateStub<S3ServiceProvider>();
            var mockSasProvider = MockRepository.GenerateStub<ISASService>();

            var mockUserService = MockRepository.GenerateStub<UserService>(mockDataAssetContext, mockExtendedUserInfoProvider, mockCurrentUserIdProvider);
            var mockSharedContextModel = MockRepository.GenerateStub<SharedContextModel>();

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
                mockDatasetContext.Stub(x => x.IsUserSubscribedToDataset(ds.SentryOwnerName, ds.DatasetId)).Return(true);
                mockDatasetContext.Stub(x => x.GetAllUserSubscriptionsForDataset(user.AssociateId, ds.DatasetId)).Return(datasetSubscriptions == null ? new List<DatasetSubscription>() : datasetSubscriptions);

                if (ds.DatasetFileConfigs.Any())
                {
                    mockDatasetContext.Stub(x => x.Merge<DatasetFileConfig>(ds.DatasetFileConfigs[0])).Return(ds.DatasetFileConfigs[0]);
                }
            }

            mockDatasetContext.Stub(x => x.Merge<Dataset>(ds)).Return(ds);     

            mockDatasetContext.Stub(x => x.EventTypes).Return(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Stub(x => x.GetAllIntervals()).Return(MockClasses.MockIntervals());
            mockDatasetContext.Stub(x => x.GetInterval("Never")).Return(MockClasses.MockIntervals().FirstOrDefault(x =>x.Description == "Never"));
            mockDatasetContext.Stub(x => x.EventStatus).Return(MockClasses.MockEventStatuses().AsQueryable());
            mockDatasetContext.Stub(x => x.FileExtensions).Return(MockClasses.MockFileExtensions().AsQueryable());
            mockDatasetContext.Stub(x => x.DataSources).Return(MockClasses.MockDataSources().AsQueryable());
            mockDatasetContext.Stub(x => x.Categories).Return(MockClasses.MockCategories().AsQueryable());
            mockDatasetContext.Stub(x => x.GetCategoryById(0)).Return(MockClasses.MockCategories()[0]);
            mockDatasetContext.Stub(x => x.GetAllDatasetScopeTypes()).Return(MockClasses.MockScopeTypes());
            mockDatasetContext.Stub(x => x.isDatasetNameDuplicate(ds.DatasetName, ds.Category)).Return(false);

            mockUserService.Stub(x => x.GetCurrentUser()).Return(user);

            var dsc = new DatasetController(mockDatasetContext, mockS3Provider, mockUserService, mockSasProvider, mockAssociateService, mockRequestService);
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

            var mockUserService = MockRepository.GenerateStub<UserService>(mockDataAssetContext, mockExtendedUserInfoProvider, mockCurrentUserIdProvider);
            var mockSharedContextModel = MockRepository.GenerateStub<SharedContextModel>();

            mockSharedContextModel.CurrentUser = user;

            var ds = dfc.ParentDataset;

            mockAssociateService.Stub(x => x.GetAssociateInfo(ds.SentryOwnerName)).Return(new Associates.Associate() { FullName = "Bill Nye" });

            if (ds != null)
            {
                List<Dataset> dsList = new List<Dataset>();
                dsList.Add(ds);

                mockDatasetContext.Stub(x => x.GetById(ds.DatasetId)).Return(ds);
                mockDatasetContext.Stub(x => x.GetById<Dataset>(ds.DatasetId)).Return(ds);
                mockDatasetContext.Stub(x => x.getDatasetFileConfigs(dfc.ConfigId)).Return(dfc);
                mockDatasetContext.Stub(x => x.GetAllDatasetScopeTypes()).Return(new List<DatasetScopeType>());
                mockDatasetContext.Stub(x => x.IsUserSubscribedToDataset(ds.SentryOwnerName, ds.DatasetId)).Return(true);
                mockDatasetContext.Stub(x => x.GetAllUserSubscriptionsForDataset(ds.SentryOwnerName, ds.DatasetId)).Return(new List<DatasetSubscription>());
            }

            mockDatasetContext.Stub(x => x.EventTypes).Return(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Stub(x => x.EventStatus).Return(MockClasses.MockEventStatuses().AsQueryable());
            mockDatasetContext.Stub(x => x.FileExtensions).Return(MockClasses.MockFileExtensions().AsQueryable());

            mockUserService.Stub(x => x.GetCurrentUser()).Return(user != null ? user : MockUsers.App_DataMgmt_Admin_User());

            var cc = new ConfigController(mockDatasetContext, mockS3Provider, mockUserService, mockSasProvider, mockAssociateService);
            cc.SharedContext = mockSharedContextModel;

            return cc;
        }
    }
}