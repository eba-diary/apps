using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.Controllers;
using Sentry.data.Core;
using Rhino.Mocks;
using System.Web.Mvc;
using System.Collections.Generic;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class DataAssetTests
    {
        [TestMethod]
        public void Index_Returns_Index_View_With_ID_0()
        {
            var dac = GetMockProviderForIndex(GetMockData());

            var result = dac.Index(0) as ViewResult;

            Assert.AreEqual("", result.ViewName);
        }

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

            return da;
        }

        public DataAssetController GetMockProviderForIndex(DataAsset da)
        {
            List<DataAsset> daList = new List<DataAsset>();
            daList.Add(da);

            var mockDataAssetProvider = MockRepository.GenerateStub<IDataAssetProvider>();
            var mockMetadataRepositoryService = MockRepository.GenerateStub<MetadataRepositoryService>();
            var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();
            var mockAssociateService = MockRepository.GenerateStub<IAssociateInfoProvider>();
            mockDataAssetProvider.Stub(x => x.GetDataAsset(da.Id)).Return(da);
            mockDataAssetProvider.Stub(x => x.GetDataAssets()).Return(daList);

            var dac = new DataAssetController(mockDataAssetProvider, mockMetadataRepositoryService, mockDatasetContext, mockAssociateService);

            return dac;
        }

        public DataAssetController GetMockProviderForName(DataAsset da)
        {
            List<DataAsset> daList = new List<DataAsset>();
            daList.Add(da);

            var mockDataAssetProvider = MockRepository.GenerateStub<IDataAssetProvider>();
            var mockMetadataRepositoryService = MockRepository.GenerateStub<MetadataRepositoryService>();
            var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();
            var mockAssociateService = MockRepository.GenerateStub<IAssociateInfoProvider>();
            mockDataAssetProvider.Stub(x => x.GetDataAsset(da.Name)).Return(da);
            mockDataAssetProvider.Stub(x => x.GetDataAssets()).Return(daList);

            var dac = new DataAssetController(mockDataAssetProvider, mockMetadataRepositoryService, mockDatasetContext, mockAssociateService);

            return dac;
        }
    }
}
