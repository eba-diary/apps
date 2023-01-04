using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Sentry.data.Core;
using System.Web.Mvc;
using System.Linq;
using Sentry.data.Web.Helpers;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class DatasetTests
    {
        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_Category()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.DatasetCategories = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetCategoryRequired));
        }

        [TestCategory("Dataset")]
        [TestMethod()]
        public void Can_prevent_dataset_with_no_description()
        {
            // //// Arrange ////
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.DatasetDesc = "";
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDescriptionRequired));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_DatasetName()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.DatasetName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(GlobalConstants.ValidationErrors.NAME_IS_BLANK));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_CreationUser()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.CreationUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetCreatedByRequired));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_no_UploadUser()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.UploadUserName = null;
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetUploadedByRequired));
        }

        [TestCategory("Dataset")]
        [TestMethod]
        public void Can_prevent_dataset_with_old_Dataset_Date()
        {
            Dataset dataset1 = MockClasses.MockDataset();
            dataset1.DatasetDtm = new DateTime(1799, 1, 1);
            var vr = dataset1.ValidateForSave();
            Assert.IsTrue(vr.Contains(Dataset.ValidationErrors.datasetDateRequired));
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

            Assert.IsTrue(model.CurrentSubscriptions.Count == MockClasses.MockEventTypes().Count(x => x.Display));
            Assert.IsTrue(model.AllIntervals.Count() == MockClasses.MockIntervals().Count);
            Assert.IsTrue(model.datasetID == ds.DatasetId);
            Assert.IsTrue(model.SentryOwnerName == user.AssociateId);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        [TestCategory("Integration Tests")]
        [TestCategory("Utility")]
        public void Dataset_Controller_Utility_InstantiateJobs_For_Create_DFSBasic()
        {
            var user = MockUsers.App_DataMgmt_Admin_User();
            var ds = MockClasses.MockDataset(user, true);
            var dataSource = MockClasses.MockDataSources()[0];

            var job = Utility.InstantiateJobsForCreation(ds.DatasetFileConfigs[0], dataSource);

            Assert.IsTrue(job.DataSource.Is<DfsBasic>());
            Assert.IsTrue(job.DataSource.Name == dataSource.Name);
            Assert.IsTrue(job.DataSource.Id == dataSource.Id);
            Assert.IsTrue(job.DatasetConfig.ConfigId == ds.DatasetFileConfigs[0].ConfigId);

            Assert.IsTrue(job.JobOptions.CompressionOptions.IsCompressed == false);
            Assert.IsTrue(job.JobOptions.IsRegexSearch == true);

            Assert.IsTrue(job.IsGeneric == true);
            Assert.IsTrue(job.Schedule == "Instant");
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        [TestCategory("Integration Tests")]
        [TestCategory("Utility")]
        public void Dataset_Controller_Utility_InstantiateJobs_For_Create_S3Basic()
        {
            var user = MockUsers.App_DataMgmt_Admin_User();
            var ds = MockClasses.MockDataset(user, true);
            var dataSource = MockClasses.MockDataSources()[1];

            var job = Utility.InstantiateJobsForCreation(ds.DatasetFileConfigs[0], dataSource);

            Assert.IsTrue(job.DataSource.Is<S3Basic>());
            Assert.IsTrue(job.DataSource.Name == dataSource.Name);
            Assert.IsTrue(job.DataSource.Id == dataSource.Id);
            Assert.IsTrue(job.DatasetConfig.ConfigId == ds.DatasetFileConfigs[0].ConfigId);

            Assert.IsTrue(job.JobOptions.CompressionOptions.IsCompressed == false);
            Assert.IsTrue(job.JobOptions.IsRegexSearch == true);

            Assert.IsTrue(job.IsGeneric == true);
            Assert.IsTrue(job.Schedule == "*/1 * * * *");
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        [TestCategory("Integration Tests")]
        [TestCategory("Utility")]
        public void Dataset_Controller_Utility_InstantiateJobs_For_Create_FTPBasic()
        {
            var user = MockUsers.App_DataMgmt_Admin_User();
            var ds = MockClasses.MockDataset(user, true);
            var dataSource = MockClasses.MockDataSources()[2];

            try
            {
                var job = Utility.InstantiateJobsForCreation(ds.DatasetFileConfigs[0], dataSource);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is NotImplementedException);
            }



        }
    }
}
