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
        [TestCategory("Data Asset Controller")]
        public void Index_Returns_Index_View_With_Valid_ID()
        {
            var dac = MockControllers.MockDataAssetController(MockClasses.MockDataAsset());

            var result = dac.Index(1) as ViewResult;

            Assert.AreEqual("", result.ViewName);
        }

        [TestMethod]
        [TestCategory("Data Asset Controller")]
        public void Index_Does_RedirectToAction_NotFound()
        {
            var dac = MockControllers.MockDataAssetController(MockClasses.MockDataAsset());

            var result = dac.Index(-1) as RedirectToRouteResult;

            Assert.AreEqual("NotFound", result.RouteValues["action"]);
            Assert.AreEqual("Error", result.RouteValues["controller"]);
        }

        [TestMethod]
        [TestCategory("Data Asset Controller")]
        public void DataAsset_Returns_Index_View_Given_No_Name()
        {
            var dac = MockControllers.MockDataAssetController(MockClasses.MockDataAsset());

            var result = dac.DataAsset(null) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        [TestCategory("Data Asset Controller")]
        public void DataAsset_Returns_Index_View_Given_Valid_Name()
        {
            var dac = MockControllers.MockDataAssetController(MockClasses.MockDataAsset());

            var result = dac.DataAsset("MockAsset") as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        [TestCategory("Data Asset Controller")]
        public void DataAsset_Does_RedirectToAction_NotFound()
        {
            var dac = MockControllers.MockDataAssetController(MockClasses.MockDataAsset());

            var result = dac.DataAsset("Mock Basset") as RedirectToRouteResult;

            Assert.AreEqual("NotFound", result.RouteValues["action"]);
            Assert.AreEqual("Error", result.RouteValues["controller"]);
        }

        [TestMethod]
        [TestCategory("Data Asset Notifications")]
        public void DataAssetNotification_IsNotInPast()
        {
            var da = MockClasses.MockDataAsset();

            var an = MockClasses.GetMockAssetNotifications(da);

            Assert.IsTrue(an.StartTime >= DateTime.Now.AddHours(-1));
            Assert.IsTrue(an.StartTime <= an.ExpirationTime);

            Assert.IsTrue(an.ExpirationTime >= an.StartTime);
            Assert.IsTrue(an.ExpirationTime >= DateTime.Now);
        }
    }
}
