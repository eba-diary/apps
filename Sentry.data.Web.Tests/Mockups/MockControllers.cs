using Rhino.Mocks;
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
        public static DataAssetController MockDataAssetController(DataAsset da)
        {
            List<DataAsset> daList = new List<DataAsset>();
            daList.Add(da);

            var mockDataAssetProvider = MockRepository.GenerateStub<IDataAssetProvider>();
            var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();
            var mockAssociateService = MockRepository.GenerateStub<IAssociateInfoProvider>();
            var mockExtendedUserInfoProvider = MockRepository.GenerateStub<IExtendedUserInfoProvider>();
            var mockCurrentUserIdProvider = MockRepository.GenerateStub<ICurrentUserIdProvider>();
            var mockMetadataRepositoryProvider = MockRepository.GenerateStub<IMetadataRepositoryProvider>();
            var mockDataAssetContext = MockRepository.GenerateStub<IDataAssetContext>();
            var mockSharedContextModel = MockRepository.GenerateStub<SharedContextModel>();

            var mockMetadataRepositoryService = MockRepository.GenerateStub<MetadataRepositoryService>(mockMetadataRepositoryProvider);
            var mockUserService = MockRepository.GenerateStub<UserService>(mockDataAssetContext, mockExtendedUserInfoProvider, mockCurrentUserIdProvider);

            mockDatasetContext.Stub(x => x.GetDataAsset(da.Name)).Return(da);
            mockDatasetContext.Stub(x => x.GetDataAsset(da.Id)).Return(da);
            mockDatasetContext.Stub(x => x.GetDataAssets()).Return(daList);
            mockDatasetContext.Stub(x => x.GetAssetNotificationsByDataAssetId(da.Id)).Return(da.AssetNotifications);

            mockDatasetContext.Stub(x => x.EventTypes).Return(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Stub(x => x.EventStatus).Return(MockClasses.MockEventStatuses().AsQueryable());

            mockSharedContextModel.CurrentUser = MockUsers.App_DataMgmt_Admin_User();

            var dac = new DataAssetController(mockDataAssetProvider, mockMetadataRepositoryService, mockDatasetContext, mockAssociateService, mockUserService);
            dac.SharedContext = mockSharedContextModel;

            return dac;
        }

        public static DatasetController MockDatasetController(Dataset ds, IApplicationUser user)
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
            
            mockAssociateService.Stub(x => x.GetAssociateInfo(user.AssociateId)).Return(new Associates.Associate() { FullName = "Bill Nye" });

            if (ds != null)
            {
                List<Dataset> dsList = new List<Dataset>();
                dsList.Add(ds);

                mockDatasetContext.Stub(x => x.GetById(ds.DatasetId)).Return(ds);
                mockDatasetContext.Stub(x => x.IsUserSubscribedToDataset(ds.SentryOwnerName, ds.DatasetId)).Return(true);
                mockDatasetContext.Stub(x => x.GetAllUserSubscriptionsForDataset(ds.SentryOwnerName, ds.DatasetId)).Return(new List<DatasetSubscription>());
            }

            mockDatasetContext.Stub(x => x.EventTypes).Return(MockClasses.MockEventTypes().AsQueryable());
            mockDatasetContext.Stub(x => x.EventStatus).Return(MockClasses.MockEventStatuses().AsQueryable());
            mockDatasetContext.Stub(x => x.FileExtensions).Return(MockClasses.MockFileExtensions().AsQueryable());

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

            mockUserService.Stub(x => x.GetCurrentUser()).Return(user);

            var cc = new ConfigController(mockDatasetContext, mockS3Provider, mockUserService, mockSasProvider, mockAssociateService);
            cc.SharedContext = mockSharedContextModel;

            return cc;
        }
    }
}
