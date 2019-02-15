﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.Controllers;
using Sentry.data.Core;
using Rhino.Mocks;
using System.Web.Mvc;
using System.Collections.Generic;
using Sentry.data.Infrastructure;
using System;
using System.Linq;
using Sentry.data.Web.Helpers;

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
        public void DataAsset_Notification_IsNotInPast()
        {
            var da = MockClasses.MockDataAsset();

            var an = MockClasses.GetMockAssetNotifications(da);

            Assert.IsTrue(an.StartTime >= DateTime.Now.AddHours(-1));
            Assert.IsTrue(an.StartTime <= an.ExpirationTime);

            Assert.IsTrue(an.ExpirationTime >= an.StartTime);
            Assert.IsTrue(an.ExpirationTime >= DateTime.Now);
        }

        [TestMethod]
        [TestCategory("Data Asset Notifications")]
        public void DataAsset_Notification_Correct_Parentage()
        {
            var da = MockClasses.MockDataAsset();

            var an = MockClasses.GetMockAssetNotifications(da);

            Assert.IsTrue(an.ParentDataAsset.Id == da.Id);
        }

        [TestMethod]
        [TestCategory("Data Asset Notifications")]
        public void DataAsset_Notification_Correct_Creator()
        {
            var da = MockClasses.MockDataAsset();

            var user = MockUsers.App_DataMgmt_MgAlert();

            var an = MockClasses.GetMockAssetNotifications(da, user);

            Assert.IsTrue(an.CreateUser == user.AssociateId);
        }

        //[TestMethod]
        //[TestCategory("Data Asset Controller")]
        //[TestCategory("Data Asset Notifications")]
        //public void DataAsset_Create_Notification_Check_Redirect()
        //{
        //    var user = MockUsers.App_DataMgmt_MgAlert();
        //    var da = MockClasses.MockDataAsset();
        //    var dac = MockControllers.MockDataAssetController(da, user);
        //    var an = MockClasses.GetMockAssetNotifications(da, user);

        //    an.Message = "Data Asset Notification Creation Test";
        //    CreateAssetNotificationModel can = new CreateAssetNotificationModel(an, dac._associateInfoService);
        //    can.SeverityID = 0;
        //    can.DataAssetID = da.Id;

        //    var result = dac.CreateAssetNotification(can);

        //    Assert.AreEqual("ManageAssetNotification", (result as ViewResult).ViewName);

        //}


        //[TestMethod]
        //[TestCategory("Data Asset Controller")]
        //[TestCategory("Data Asset Notifications")]
        //public void DataAsset_Create_Notification_Model()
        //{
        //    var user = MockUsers.App_DataMgmt_MgAlert();
        //    var da = MockClasses.MockDataAsset();
        //    var dac = MockControllers.MockDataAssetController(da, user);
        //    var an = MockClasses.GetMockAssetNotifications(da, user);

        //    CreateAssetNotificationModel can = new CreateAssetNotificationModel();
        //    can.SeverityID = 0;
        //    can.Message = "Data Asset Notification Creation Test";
        //    can.CreateUser = user.AssociateId;
        //    can.StartTime = an.StartTime;
        //    can.ExpirationTime = an.ExpirationTime;
        //    can.DataAssetID = da.Id;

        //    var returnedNotification = new AssetNotifications();

        //    returnedNotification = dac.CreateAssetNotificationFromModel(returnedNotification, can);

        //    Assert.IsTrue(da.Name.ToString() == returnedNotification.ParentDataAsset.Name.ToString());

        //}

        //[TestMethod]
        //[TestCategory("Data Asset Controller")]
        //[TestCategory("Data Asset Notifications")]
        //public void DataAsset_Edit_Notification_Model()
        //{
        //    var user = MockUsers.App_DataMgmt_MgAlert();
        //    var da = MockClasses.MockDataAsset();
        //    var dac = MockControllers.MockDataAssetController(da, user);
        //    var an = MockClasses.GetMockAssetNotifications(da, user);

        //    EditAssetNotificationModel ean = new EditAssetNotificationModel();
        //    ean.SeverityID = an.MessageSeverity + 1;
        //    ean.Message = "Data Asset Notification Edit Test";
        //    ean.ExpirationTime = an.ExpirationTime.AddHours(1);


        //    var returnedNotification = dac.UpdateAssetNotificationFromModel(an, ean);

        //    Assert.IsTrue(da.Name.ToString() == returnedNotification.ParentDataAsset.Name.ToString());

        //    Assert.IsTrue(returnedNotification.ExpirationTime == ean.ExpirationTime);
        //    Assert.IsFalse(returnedNotification.ExpirationTime == MockClasses.GetMockAssetNotifications(da, user).ExpirationTime);

        //    Assert.IsTrue(returnedNotification.Message == ean.Message);
        //    Assert.IsFalse(returnedNotification.Message == MockClasses.GetMockAssetNotifications(da, user).Message);

        //    Assert.IsTrue(returnedNotification.MessageSeverity == ean.SeverityID);
        //    Assert.IsFalse(returnedNotification.MessageSeverity == MockClasses.GetMockAssetNotifications(da, user).MessageSeverity);

        //}

        [TestMethod]
        [TestCategory("Data Asset Notifications")]
        public void DataAsset_Notification_DisplayMessage()
        {
            var user = MockUsers.App_DataMgmt_MgAlert();
            var da = MockClasses.MockDataAsset();
            var dac = MockControllers.MockDataAssetController(da, user);
            var an = MockClasses.GetMockAssetNotifications(da, user);

            string mock = "This is a Mock Test.  Way different then the Mock.";
            an.Message = mock;
            an.MessageSeverity = NotificationSeverity.Danger;

            Assert.IsTrue(an.MessageSeverityTag == NotificationSeverity.Danger.ToString());
            Assert.IsTrue(an.DisplayMessage.Contains("Alert!"));
            Assert.IsTrue(an.DisplayMessage.EndsWith(mock));


        }


        [TestMethod]
        [TestCategory("Data Asset Detail")]
        public void TimeDisplay_HourCheck()
        {

            DateTime dateTime = DateTime.Now.AddHours(-20);

            String display = Utility.TimeDisplay(dateTime);

            Assert.IsTrue(display == "20 hours ago");



        }

        [TestMethod]
        [TestCategory("Data Asset Detail")]
        public void TimeDisplay_DaysCheck()
        {

            DateTime dateTime = DateTime.Now.AddDays(-20);

            String display = Utility.TimeDisplay(dateTime);

            Assert.IsTrue(display == dateTime.ToString("MM/dd/yyyy hh:mm:ss tt"));

        }


        [TestMethod]
        [TestCategory("Data Asset Detail")]
        public void TimeDisplay_DaysCheckFromTimeStamp()
        {
            DateTime acclaim = Convert.ToDateTime("2018-07-31 08:37:51.000");

            String display = Utility.TimeDisplay(acclaim);

            Assert.IsTrue(display == acclaim.ToString("MM/dd/yyyy hh:mm:ss tt"));
        }

    }
}
