using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Sentry.data.Core;
using Sentry.data.Web.Tests;
using System.Web.Mvc;
using Sentry.data.Web;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class DatasetTests
    {
        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_s3Key()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.S3Key = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.s3keyIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_Category()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.Category = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.categoryIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod()]
        public void Can_prevent_dataset_with_no_description()
        {
            // //// Arrange ////
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.DatasetDesc = "";
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDescIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_DatasetName()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.DatasetName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.nameIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_CreationUser()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.CreationUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.creationUserNameIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_UploadUser()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.UploadUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.uploadUserNameIsBlank));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_old_Dataset_Date()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.DatasetDtm = new DateTime(1799, 1, 1);
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDateIsOld));
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        public void Dataset_Controller_Index_Model_Check_Index_URL()
        {
            var user = MockUsers.App_DataMgmt_Upld();
            var ds = MockClasses.MockDataset();
            var dac = MockControllers.MockDatasetController(ds, user);

            var result = dac.Index() as ViewResult;

            //We don't want to see Index in the URL
            Assert.AreEqual("", result.ViewName);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        public void Dataset_Controller_Index_Model_Check_Default_User()
        {
            var user = MockUsers.App_DataMgmt_Upld();
            var ds = MockClasses.MockDataset();
            var dac = MockControllers.MockDatasetController(ds, user);

            var result = dac.Index() as ViewResult;

            Assert.IsTrue(result.Model.GetType() == typeof(HomeModel));

            var model = (result.Model as HomeModel);

            Assert.IsTrue(model.DatasetCount == 1);
            Assert.IsTrue(model.Categories.Count == 1);
            Assert.IsTrue(model.CanEditDataset == false);
            Assert.IsTrue(model.CanUpload == true);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        public void Dataset_Controller_Index_Model_Check_Manage_Dataset_User()
        {
            var user = MockUsers.App_DataMgmt_MngDS();
            var ds = MockClasses.MockDataset();
            var dac = MockControllers.MockDatasetController(ds, user);

            var result = dac.Index() as ViewResult;

            Assert.IsTrue(result.Model.GetType() == typeof(HomeModel));

            var model = (result.Model as HomeModel);

            Assert.IsTrue(model.DatasetCount == 1);
            Assert.IsTrue(model.Categories.Count == 1);
            Assert.IsTrue(model.CanEditDataset == true);
            Assert.IsTrue(model.CanUpload == true);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        public void Dataset_Controller_Create_Without_Model_Check_Create_URL()
        {
            var user = MockUsers.App_DataMgmt_Upld();
            var ds = MockClasses.MockDataset();
            var dac = MockControllers.MockDatasetController(ds, user);

            var result = dac.Create() as ViewResult;

            //We don't want to see Index in the URL
            Assert.AreEqual("", result.ViewName);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        public void Dataset_Controller_Create_Without_Model_Check_Default_User()
        {
            var user = MockUsers.App_DataMgmt_Upld();
            var ds = MockClasses.MockDataset();
            var dac = MockControllers.MockDatasetController(ds, user);

            var result = dac.Create() as ViewResult;

            Assert.IsTrue(result.Model.GetType() == typeof(CreateDatasetModel));

            var model = (result.Model as CreateDatasetModel);

            Assert.IsTrue(model.CanEditDataset == false);
            Assert.IsTrue(model.CanUpload == true);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        public void Dataset_Controller_Create_Without_Model_Check_Manage_Dataset_User()
        {
            var user = MockUsers.App_DataMgmt_MngDS();
            var ds = MockClasses.MockDataset();
            var dsc = MockControllers.MockDatasetController(ds, user);

            var result = dsc.Create() as ViewResult;

            Assert.IsTrue(result.Model.GetType() == typeof(CreateDatasetModel));

            var model = (result.Model as CreateDatasetModel);

            Assert.IsTrue(model.CanEditDataset == true);
            Assert.IsTrue(model.CanUpload == true);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        [TestCategory("Dataset Subscriptions")]
        public void Dataset_Controller_Subscribe_Check_Current_Subscriptions_With_Subs()
        {
            var user = MockUsers.App_DataMgmt_MngDS();
            var ds = MockClasses.MockDataset();
            var subs = MockClasses.MockDatasetSubscriptions(ds, user);
            var dsc = MockControllers.MockDatasetController(ds, user, subs);

            var result = dsc.Subscribe(ds.DatasetId) as PartialViewResult;

            Assert.IsTrue(result.Model.GetType() == typeof(SubscriptionModel));

            var model = (result.Model as SubscriptionModel);

            Assert.IsTrue(model.CurrentSubscriptions.Any(x => x.Interval.Description != "Never")); //There should be one that is labeled Daily
            Assert.IsTrue(model.CurrentSubscriptions.All(x => x.SentryOwnerName == user.AssociateId));  //Every subscription returned should be belonging to the current user.

            Assert.IsTrue(model.SentryOwnerName == user.AssociateId);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        [TestCategory("Dataset Subscriptions")]
        public void Dataset_Controller_Subscribe_Check_Current_Subscriptions_Without_Subs()
        {
            var user = MockUsers.App_DataMgmt_Upld();
            var ds = MockClasses.MockDataset();
            var dsc = MockControllers.MockDatasetController(ds, user);

            var result = dsc.Subscribe(ds.DatasetId) as PartialViewResult;

            Assert.IsTrue(result.Model.GetType() == typeof(SubscriptionModel));

            var model = (result.Model as SubscriptionModel);

            Assert.IsTrue(model.CurrentSubscriptions.All(x => x.Interval.Description == "Never")); //There should be NONE that are labeled Daily
            Assert.IsTrue(model.CurrentSubscriptions.All(x => x.SentryOwnerName == user.AssociateId));  //Every subscription returned should be belonging to the current user.

            Assert.IsTrue(model.SentryOwnerName == user.AssociateId);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        [TestCategory("Dataset Subscriptions")]
        public void Dataset_Controller_Subscribe_Check_Model()
        {
            var user = MockUsers.App_DataMgmt_Admin_User();
            var ds = MockClasses.MockDataset();
            var subs = MockClasses.MockDatasetSubscriptions(ds, user);
            var dsc = MockControllers.MockDatasetController(ds, user, subs);

            var result = dsc.Subscribe(ds.DatasetId) as PartialViewResult;

            Assert.IsTrue(result.Model.GetType() == typeof(SubscriptionModel));

            var model = (result.Model as SubscriptionModel);

            Assert.IsTrue(model.CurrentSubscriptions.Count == MockClasses.MockEventTypes().Where(x => x.Display).Count());
            Assert.IsTrue(model.AllEventTypes.Count() == MockClasses.MockEventTypes().Where(x => x.Display).Count());
            Assert.IsTrue(model.AllIntervals.Count() == MockClasses.MockIntervals().Count);

            Assert.IsTrue(model.datasetID == ds.DatasetId);
            Assert.IsTrue(model.SentryOwnerName == user.AssociateId);
        }

    }
}
