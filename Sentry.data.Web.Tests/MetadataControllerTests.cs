using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class MetadataControllerTests : BaseWebUnitTests
    {

        [TestInitialize]
        public void MyTestInitialize()
        {
            TestInitialize();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TestCleanup();
        }

        [TestMethod]
        public void MigrationRequest_ToDto()
        {
            Models.ApiModels.Migration.DatasetMigrationRequestModel model = new Models.ApiModels.Migration.DatasetMigrationRequestModel()
            {
                SourceDatasetId = 99,
                TargetDatasetNamedEnvironment = "TEST",
                TargetDatasetId = 22,
                SchemaMigrationRequests = new List<Models.ApiModels.Migration.SchemaMigrationRequestModel>()
                {
                    new SchemaMigrationRequestModel()
                    {
                        SourceSchemaId = 44,
                        TargetDatasetNamedEnvironment = "TEST",
                        TargetDataFlowNamedEnviornment = "TEST2",
                        TargetDatasetId = 33
                    },
                    new SchemaMigrationRequestModel()
                    {
                        SourceSchemaId = 66,
                        TargetDatasetNamedEnvironment = "TEST",
                        TargetDataFlowNamedEnviornment = "TEST2",
                        TargetDatasetId = 33
                    },
                    new SchemaMigrationRequestModel()
                    {
                        SourceSchemaId = 88,
                        TargetDatasetNamedEnvironment = "TEST",
                        TargetDataFlowNamedEnviornment = "TEST2",
                        TargetDatasetId = 33
                    }
                }
            };

        //    Dataset ds1 = MockClasses.MockDataset();
        //    DatasetFileConfig dfc1 = MockClasses.MockDataFileConfig(ds1);
        //    var user1 = MockUsers.App_DataMgmt_Admin_User();

        //    var mockDatasetContext = MockRepository.GenerateStub<IDatasetContext>();
        //    mockDatasetContext.Stub(x => x.GetById(ds1.DatasetId)).Return(ds1).Repeat.Any();
        //    mockDatasetContext.Stub(x => x.GetById<Dataset>(ds1.DatasetId)).Return(ds1).Repeat.Any();
        //    mockDatasetContext.Stub(x => x.GetById<DatasetFileConfig>(dfc1.ConfigId)).Return(dfc1);

        //    _container.Inject(mockDatasetContext);

        //    //UserSecurity us = new UserSecurity();
        //    //ISecurityService mockSecurityService = _container.GetInstance<ISecurityService>();
        //    //mockSecurityService.Stub(x => x.GetUserSecurity(ds1, user1)).Return(MockClasses.GetMockUserSecurity_Public_Admin());

        //    //ISecurable securable = MockRepository.GenerateMock<ISecurable>();
        //    //securable.Stub(x => x.IsSecured).Return(false).Repeat.Any();
        //    //securable.Stub(x => x.PrimaryOwnerId).Return(ds1.PrimaryOwnerId).Repeat.Any();


        //    var mc = MockControllers.MockMetadataController(ds1, dfc1, null, user1);



        //    try
        //    {
        //        var job = await mc.GetSchemaRevisionBySchema(ds1.DatasetId, 9999);
        //        Assert.Fail();
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.IsTrue(ex is HttpNotFoundResult);
        //    }

        //}
    }
}
