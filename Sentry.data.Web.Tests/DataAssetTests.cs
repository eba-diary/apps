using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.Controllers;
using Sentry.data.Core;
using Rhino.Mocks;
using System.Web.Mvc;
using System.Collections.Generic;
using Sentry.data.Infrastructure;
using System;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class DataAssetTests
    {
        [TestMethod]
        public void Index_Returns_Index_View_With_Valid_ID()
        {
            var dac = GetMockProviderForIndex(GetMockData());

            var result = dac.Index(1) as ViewResult;

            Assert.AreEqual("", result.ViewName);
        }

        [TestMethod]
        public void Index_Does_RedirectToAction_NotFound()
        {
            var dac = GetMockProviderForIndex(GetMockData());

            var result = dac.Index(-1) as RedirectToRouteResult;

            Assert.AreEqual("NotFound", result.RouteValues["action"]);
            Assert.AreEqual("Error", result.RouteValues["controller"]);
        }

        [TestMethod]
        public void DataAsset_Returns_Index_View_Given_No_Name()
        {
            var dac = GetMockProviderForName(GetMockData());

            var result = dac.DataAsset(null) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void DataAsset_Returns_Index_View_Given_Valid_Name()
        {
            var dac = GetMockProviderForName(GetMockData());

            var result = dac.DataAsset("MockAsset") as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void DataAsset_Does_RedirectToAction_NotFound()
        {
            var dac = GetMockProviderForName(GetMockData());

            var result = dac.DataAsset("Mock Basset") as RedirectToRouteResult;

            Assert.AreEqual("NotFound", result.RouteValues["action"]);
            Assert.AreEqual("Error", result.RouteValues["controller"]);
        }

        public DataAsset GetMockData()
        {
            DataAsset da = new DataAsset();
            da.Id = 1;
            da.Name = "MockAsset";
            da.DisplayName = "Mock Asset";
            da.Description = "This is a description";

            List<AssetNotifications> anList = new List<AssetNotifications>();
            anList.Add(GetMockData(da));

            da.AssetNotifications = anList;
            
            return da;
        }

        public AssetNotifications GetMockData(DataAsset da)
        {
            AssetNotifications an = new AssetNotifications();
            an.NotificationId = 1;
            an.ParentDataAsset = da;
            an.Message = "Alert Message";
            an.StartTime = DateTime.Now.AddHours(-1).AddMinutes(1);
            an.ExpirationTime = DateTime.Now.AddDays(1);

            return an;
        }

        [TestMethod]
        public void DataAssetNotification_IsNotInPast()
        {
            var da = GetMockData();

            var an = GetMockData(da);

            Assert.IsTrue(an.StartTime >= DateTime.Now.AddHours(-1));
            Assert.IsTrue(an.StartTime <= an.ExpirationTime);

            Assert.IsTrue(an.ExpirationTime >= an.StartTime);
            Assert.IsTrue(an.ExpirationTime >= DateTime.Now);
        }


        public DataAssetController GetMockProviderForIndex(DataAsset da)
        {
            DomainUser domainUser1 = new DomainUser("123456");

            List<DataAsset> daList = new List<DataAsset>();
            daList.Add(da);

            var mockDataAssetProvider = MockRepository.GenerateStub<IDataAssetProvider>();
            var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();
            var mockAssociateService = MockRepository.GenerateStub<IAssociateInfoProvider>();
            var mockExtendedUserInfoProvider = MockRepository.GenerateStub<IExtendedUserInfoProvider>();
            var mockCurrentUserIdProvider = MockRepository.GenerateStub<ICurrentUserIdProvider>();
            var mockMetadataRepositoryProvider = MockRepository.GenerateStub<IMetadataRepositoryProvider>();
            var mockDataAssetContext = MockRepository.GenerateStub<IDataAssetContext>();

            var mockMetadataRepositoryService = MockRepository.GenerateStub<MetadataRepositoryService>(mockMetadataRepositoryProvider);
            var mockUserService = MockRepository.GenerateStub<UserService>(mockDataAssetContext, mockExtendedUserInfoProvider, mockCurrentUserIdProvider);
            var mockSharedContextModel = MockRepository.GenerateStub<SharedContextModel>();            

            mockDatasetContext.Stub(x => x.GetDataAsset(da.Id)).Return(da);
            mockDatasetContext.Stub(x => x.GetDataAssets()).Return(daList);
            mockDatasetContext.Stub(x => x.GetAssetNotificationsByDataAssetId(da.Id)).Return(da.AssetNotifications);
            mockSharedContextModel.CurrentUser = GetMockAdminUser();

            var dac = new DataAssetController(mockDataAssetProvider, mockMetadataRepositoryService, mockDatasetContext, mockAssociateService, mockUserService);
            dac.SharedContext = mockSharedContextModel;                      

            return dac;
        }

        public DataAssetController GetMockProviderForName(DataAsset da)
        {
            List<DataAsset> daList = new List<DataAsset>();
            List<AssetNotifications> anList = new List<AssetNotifications>();
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
            mockDatasetContext.Stub(x => x.GetDataAssets()).Return(daList);
            mockDatasetContext.Stub(x => x.GetAssetNotificationsByDataAssetId(da.Id)).Return(da.AssetNotifications);
            mockSharedContextModel.CurrentUser = GetMockAdminUser();

            var dac = new DataAssetController(mockDataAssetProvider, mockMetadataRepositoryService, mockDatasetContext, mockAssociateService, mockUserService);
            dac.SharedContext = mockSharedContextModel;

            return dac;
        }

        public IApplicationUser GetMockAdminUser()
        {
            var user1 = MockRepository.GenerateStub<IApplicationUser>();

            user1.Stub(x => x.EmailAddress).Return("user2@b.com");

            user1.Stub(x => x.CanUserSwitch).Return(true);

            return user1;
        }
    }
}
