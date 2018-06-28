using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.data.Web.Controllers;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        [TestCategory("Integration Tests")]
        [TestMethod]
        public void Add_Data_File_Job_and_Config_To_Dataset()
        {
            var user = MockUsers.App_DataMgmt_Upld();
            var ds = MockClasses.MockDataset(user);

            Assert.IsTrue(user.CanUpload);
            Assert.IsTrue(ds.ValidateForSave().IsValid());

            var dfc = MockClasses.MockDataFileConfig(ds);
            var job = MockClasses.GetMockRetrieverJob(dfc);

            dfc.RetrieverJobs.Add(job);
            ds.DatasetFileConfigs.Add(dfc);

            Assert.IsTrue(job.ReadableSchedule == "Instant");

            var df = MockClasses.MockDataFile(ds, dfc, user);

            Assert.IsFalse(job.FilterIncomingFile(df.FileName));

            ds.DatasetFiles.Add(df);

            Assert.IsTrue(ds.DatasetFiles.Count == 1);
            Assert.IsTrue(ds.DatasetFileConfigs.Count == 1);

            Assert.IsTrue(ds.DatasetId == 1000);
            Assert.IsTrue(dfc.ConfigId == 2000);
            Assert.IsTrue(df.DatasetFileId == 3000);
            Assert.IsTrue(job.Id == 4000);

            Assert.IsTrue(ds.DatasetFiles[0].DatasetFileConfig.ConfigId == ds.DatasetFileConfigs[0].ConfigId);
            Assert.IsTrue(ds.DatasetScopeType[0].ScopeTypeId == ds.DatasetFileConfigs[0].DatasetScopeType.ScopeTypeId);
            Assert.IsTrue(ds.DatasetFileConfigs[0].ConfigId == job.DatasetConfig.ConfigId);

            Assert.AreSame(ds, df.Dataset);
            Assert.AreSame(ds, dfc.ParentDataset);
            Assert.AreSame(job.DatasetConfig, dfc);
            Assert.AreSame(df.DatasetFileConfig, dfc);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        public void DatasetController_Detail_Correct_View_Normal_User()
        {
            var user = MockUsers.App_DataMgmt_Upld();

            var ds = MockClasses.MockDataset(user);
            var dfc = MockClasses.MockDataFileConfig(ds);
            var job = MockClasses.GetMockRetrieverJob(dfc);
            var df = MockClasses.MockDataFile(ds, dfc, user);

            dfc.RetrieverJobs.Add(job);
            ds.DatasetFileConfigs.Add(dfc);
            ds.DatasetFiles.Add(df);

            DatasetController dc = MockControllers.MockDatasetController(ds, user);

            var result = dc.Detail(ds.DatasetId) as ViewResult;

            Assert.IsInstanceOfType(result.Model, typeof(BaseDatasetModel));

            BaseDatasetModel model = (BaseDatasetModel) result.Model;

            Assert.IsTrue(model.IsPushToSASCompatible);
            Assert.IsTrue(model.IsPreviewCompatible);
            Assert.IsTrue(model.DatasetFileConfigs.Count == 1);
            Assert.IsTrue(model.DatasetFiles.Count == 1);

            Assert.IsTrue(model.UploadUserName == ds.UploadUserName);

            Assert.IsTrue(model.CanEditDataset == false);
            Assert.IsTrue(model.CanDwnldSenstive == false);
            Assert.IsTrue(model.CanManageConfigs == false);
            Assert.IsTrue(model.CanDwnldNonSensitive == true);
            Assert.IsTrue(model.CanQueryTool == false);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        public void DatasetController_Detail_Correct_View_Manage_Dataset_User()
        {
            var user = MockUsers.App_DataMgmt_MngDS();

            var ds = MockClasses.MockDataset(user);
            var dfc = MockClasses.MockDataFileConfig(ds);
            var job = MockClasses.GetMockRetrieverJob(dfc);
            var df = MockClasses.MockDataFile(ds, dfc, user);

            dfc.RetrieverJobs.Add(job);
            ds.DatasetFileConfigs.Add(dfc);
            ds.DatasetFiles.Add(df);

            DatasetController dc = MockControllers.MockDatasetController(ds, user);

            var result = dc.Detail(ds.DatasetId) as ViewResult;

            Assert.IsInstanceOfType(result.Model, typeof(BaseDatasetModel));

            BaseDatasetModel model = (BaseDatasetModel)result.Model;

            Assert.IsTrue(model.IsPushToSASCompatible);
            Assert.IsTrue(model.IsPreviewCompatible);
            Assert.IsTrue(model.DatasetFileConfigs.Count == 1);
            Assert.IsTrue(model.DatasetFiles.Count == 1);

            Assert.IsTrue(model.UploadUserName == ds.UploadUserName);

            Assert.IsTrue(model.CanEditDataset == true);
            Assert.IsTrue(model.CanDwnldSenstive == false);
            Assert.IsTrue(model.CanManageConfigs == true);
            Assert.IsTrue(model.CanDwnldNonSensitive == true);
            Assert.IsTrue(model.CanQueryTool == false);
        }

        [TestMethod]
        [TestCategory("Dataset Controller")]
        public void DatasetController_DatasetConfiguration_Correct_View()
        {
            var user = MockUsers.App_DataMgmt_Upld();

            var ds = MockClasses.MockDataset(user);
            var dfc = MockClasses.MockDataFileConfig(ds);
            var job = MockClasses.GetMockRetrieverJob(dfc);
            var df = MockClasses.MockDataFile(ds, dfc, user);

            dfc.RetrieverJobs.Add(job);
            ds.DatasetFileConfigs.Add(dfc);
            ds.DatasetFiles.Add(df);

            DatasetController dc = MockControllers.MockDatasetController(ds, user);


            var result = dc.DatasetConfiguration(ds.DatasetId) as ViewResult;

            Assert.IsInstanceOfType(result.Model, typeof(BaseDatasetModel));

            BaseDatasetModel model = (BaseDatasetModel)result.Model;

            Assert.IsTrue(model.IsPushToSASCompatible);
            Assert.IsTrue(model.IsPreviewCompatible);
            Assert.IsTrue(model.DatasetFileConfigs.Count == 1);
            Assert.IsTrue(model.DatasetFiles.Count == 1);

            Assert.IsTrue(model.UploadUserName == ds.UploadUserName);
            Assert.IsTrue(result.ViewName == "Configuration");
        }

        [TestMethod]
        [TestCategory("Models")]
        public void BaseDatasetModel_Creation_DistinctFrequencies()
        {
            var user = MockUsers.App_DataMgmt_Upld();

            var ds = MockClasses.MockDataset(user);
            var dfc = MockClasses.MockDataFileConfig(ds);
            var job = MockClasses.GetMockRetrieverJob(dfc);
            var df = MockClasses.MockDataFile(ds, dfc, user);

            dfc.RetrieverJobs.Add(job);
            ds.DatasetFileConfigs.Add(dfc);
            ds.DatasetFiles.Add(df);

            DatasetController dc = MockControllers.MockDatasetController(ds, user);

            BaseDatasetModel model = new BaseDatasetModel(ds, dc._associateInfoProvider);

            Assert.IsTrue(model.DistinctFrequencies().Count == 1);
        }

        [TestMethod]
        [TestCategory("Models")]
        public void BaseDatasetModel_Creation_DistinctFileExtensions()
        {
            var user = MockUsers.App_DataMgmt_Upld();

            var ds = MockClasses.MockDataset(user);
            var dfc = MockClasses.MockDataFileConfig(ds);
            var job = MockClasses.GetMockRetrieverJob(dfc);
            var df = MockClasses.MockDataFile(ds, dfc, user);

            dfc.RetrieverJobs.Add(job);
            ds.DatasetFileConfigs.Add(dfc);
            ds.DatasetFiles.Add(df);

            DatasetController dc = MockControllers.MockDatasetController(ds, user);

            BaseDatasetModel model = new BaseDatasetModel(ds, dc._associateInfoProvider);

            Assert.IsTrue(model.DistinctFileExtensions().Count == 1);
            Assert.IsTrue(model.DistinctFileExtensions()[0].ToLower() == dfc.FileExtension.Name.ToLower());
        }

        [TestMethod]
        [TestCategory("Models")]
        public void BaseDatasetModel_Creation_IsPushToSASCompatible()
        {
            var user = MockUsers.App_DataMgmt_Upld();

            var ds = MockClasses.MockDataset(user);
            var dfc = MockClasses.MockDataFileConfig(ds);
            var job = MockClasses.GetMockRetrieverJob(dfc);
            var df = MockClasses.MockDataFile(ds, dfc, user);

            dfc.RetrieverJobs.Add(job);
            ds.DatasetFileConfigs.Add(dfc);
            ds.DatasetFiles.Add(df);

            DatasetController dc = MockControllers.MockDatasetController(ds, user);

            BaseDatasetModel model = new BaseDatasetModel(ds, dc._associateInfoProvider);

            Assert.IsTrue(model.IsPushToSASCompatible);
        
        }

        [TestMethod]
        [TestCategory("Models")]
        public void BaseDatasetModel_Creation_DropLocationCount()
        {
            var user = MockUsers.App_DataMgmt_Upld();

            var ds = MockClasses.MockDataset(user);
            var dfc = MockClasses.MockDataFileConfig(ds);
            var job = MockClasses.GetMockRetrieverJob(dfc);
            var df = MockClasses.MockDataFile(ds, dfc, user);

            dfc.RetrieverJobs.Add(job);
            ds.DatasetFileConfigs.Add(dfc);
            ds.DatasetFiles.Add(df);

            DatasetController dc = MockControllers.MockDatasetController(ds, user);

            BaseDatasetModel model = new BaseDatasetModel(ds, dc._associateInfoProvider);

            Assert.IsTrue(model.DropLocations.Count == 1);
        }

        [TestMethod]
        [TestCategory("Models")]
        public void BaseDatasetModel_Creation_IsPreviewCompatible()
        {
            var user = MockUsers.App_DataMgmt_Upld();

            var ds = MockClasses.MockDataset(user);
            var dfc = MockClasses.MockDataFileConfig(ds);
            var job = MockClasses.GetMockRetrieverJob(dfc);
            var df = MockClasses.MockDataFile(ds, dfc, user);

            dfc.RetrieverJobs.Add(job);
            ds.DatasetFileConfigs.Add(dfc);
            ds.DatasetFiles.Add(df);

            DatasetController dc = MockControllers.MockDatasetController(ds, user);

            BaseDatasetModel model = new BaseDatasetModel(ds, dc._associateInfoProvider);

            Assert.IsTrue(model.IsPreviewCompatible);
        }


        [TestMethod]
        [TestCategory("Models")]
        [TestCategory("Data Asset Notifications")]
        public void BaseAssetNotificationModel_Creation_IsStarted()
        {
            var da = MockClasses.MockDataAsset();
            var an = MockClasses.GetMockAssetNotifications(da);

            DataAssetController dac = MockControllers.MockDataAssetController(da);

            var ban = new BaseAssetNotificationModel(an, dac._associateInfoService);

            Assert.IsTrue(ban.IsActive);
        }

        [TestMethod]
        [TestCategory("Models")]
        [TestCategory("Data Asset Notifications")]
        public void EditAssetNotificationModel_Creation_IsStarted()
        {
            var da = MockClasses.MockDataAsset();
            var an = MockClasses.GetMockAssetNotifications(da);

            DataAssetController dac = MockControllers.MockDataAssetController(da);

            var ean = new EditAssetNotificationModel(an, dac._associateInfoService);

            Assert.IsTrue(ean.IsActive);
        }

        [TestMethod]
        [TestCategory("Models")]
        [TestCategory("Data Asset Notifications")]
        public void EditAssetNotificationModel_Creation_Has_Access_Returns_Manage_Page_View()
        {
            var user = MockUsers.App_DataMgmt_MgAlert();
            var da = MockClasses.MockDataAsset();
            var an = MockClasses.GetMockAssetNotifications(da, user);

            DataAssetController dac = MockControllers.MockDataAssetController(da, user);

            var ean = new EditAssetNotificationModel(an, dac._associateInfoService);

            var result = dac.EditAssetNotification(ean) as ViewResult;

            //They are then sent back to the Manage Page
            Assert.IsTrue(result.ViewName == "ManageAssetNotification");
        }

        [TestMethod]
        [TestCategory("Models")]
        [TestCategory("Data Asset Notifications")]
        public void EditAssetNotificationModel_Creation_Does_NOT_have_Access_Returns_Error()
        {
            var user = MockUsers.App_DataMgmt_User();
            var da = MockClasses.MockDataAsset();
            var an = MockClasses.GetMockAssetNotifications(da, user);

            DataAssetController dac = MockControllers.MockDataAssetController(da, user);

            var ean = new EditAssetNotificationModel(an, dac._associateInfoService);

            try
            {
                dac.EditAssetNotification(ean);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is UnauthorizedAccessException);
            }
        }

        [TestMethod]
        [TestCategory("Models")]
        [TestCategory("Data Asset Notifications")]
        public void CreateAssetNotificationModel_Creation_Has_Access_Returns_Manage_Page_View()
        {
            var user = MockUsers.App_DataMgmt_MgAlert();
            var da = MockClasses.MockDataAsset();
            var an = MockClasses.GetMockAssetNotifications(da, user);

            DataAssetController dac = MockControllers.MockDataAssetController(da, user);

            var can = new CreateAssetNotificationModel(an, dac._associateInfoService);

            var result = dac.CreateAssetNotification(can) as ViewResult;

            //They are then sent back to the Manage Page
            Assert.IsTrue(result.ViewName == "ManageAssetNotification");
        }

        [TestMethod]
        [TestCategory("Models")]
        [TestCategory("Data Asset Notifications")]
        public void CreateAssetNotificationModel_Creation_Does_NOT_have_Access_Returns_Error()
        {
            var user = MockUsers.App_DataMgmt_User();
            var da = MockClasses.MockDataAsset();
            var an = MockClasses.GetMockAssetNotifications(da, user);

            DataAssetController dac = MockControllers.MockDataAssetController(da, user);

            var can = new CreateAssetNotificationModel(an, dac._associateInfoService);

            try
            {
                dac.CreateAssetNotification(can);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is UnauthorizedAccessException);
            }
        }
    }
}
