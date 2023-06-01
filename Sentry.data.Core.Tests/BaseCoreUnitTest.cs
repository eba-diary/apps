using Newtonsoft.Json.Linq;
using Moq;
using Sentry.data.Core.Interfaces.InfrastructureEventing;
using Sentry.FeatureFlags;
using Sentry.FeatureFlags.Mock;
using StructureMap;
using System;
using System.IO;

namespace Sentry.data.Core.Tests
{
    public class BaseCoreUnitTest
    {
        protected static IContainer _container;
        protected MockRepository _mockRepository;

        protected Mock<ISecurityService> _securityService;
        protected Mock<IDatasetContext> _datasetContext;
        protected Mock<IDataFeatures> _dataFeatures;
        protected Mock<IQuartermasterService> _quartermasterService;

        public virtual void TestInitialize(MockBehavior mockBehavior = MockBehavior.Loose)
        {
            StructureMap.Registry registry = new StructureMap.Registry();
            _mockRepository = new MockRepository(mockBehavior);
            _dataFeatures = _mockRepository.Create<IDataFeatures>();
            _datasetContext = _mockRepository.Create<IDatasetContext>();
            _securityService = _mockRepository.Create<ISecurityService>();
            _quartermasterService = _mockRepository.Create<IQuartermasterService>();

            //this should can all core assembly for implementations
            registry.Scan((scanner) =>
            {
                scanner.AssemblyContainingType<ISecurityService>();
                scanner.WithDefaultConventions();
            });




            //add in the infrastructure implementations using MockRepository so we don't actually initalize contexts or services.
            registry.For<IDatasetContext>().Use(() => _datasetContext.Object);
            registry.For<ITicketProvider>().Use(() => new Mock<ICherwellProvider>().Object);
            registry.For<IDataFeatures>().Use(_dataFeatures.Object);
            registry.For<IInevService>().Use(() => new Mock<IInevService>().Object);
            registry.For<IQuartermasterService>().Use(() => _quartermasterService.Object);
            registry.For<IJiraService>().Use(() => new Mock<IJiraService>().Object);
            registry.For<Hangfire.IBackgroundJobClient>().Use(() => new Mock<Hangfire.IBackgroundJobClient>().Object);
            registry.For<IObsidianService>().Use(() => new Mock<IObsidianService>().Object);
            registry.For<IAdSecurityAdminProvider>().Use(() => new Mock<IAdSecurityAdminProvider>().Object);
            registry.For<ISecurityService>().Use(() => _securityService.Object);

            //set the container
            _container = new StructureMap.Container(registry);

            //Set up very resuable datasetContext objects here if needed.

        }

        protected void TestCleanup()
        {
            _container.Dispose();
        }

        protected JObject GetData(string fileName)
        {
            using (StreamReader rdr = new StreamReader($@"ExpectedJSON\{fileName}"))
            {
                return JObject.Parse(rdr.ReadToEnd().Replace("\r\n", string.Empty));
            }
        }

        public class MockDataFeatures : IDataFeatures
        {
            public IFeatureFlag<bool> Remove_Mock_Uncompress_Logic_CLA_759 => throw new NotImplementedException();
            public IFeatureFlag<bool> Remove_ConvertToParquet_Logic_CLA_747 => throw new NotImplementedException();
            public IFeatureFlag<bool> Remove_Mock_GoogleAPI_Logic_CLA_1679 => throw new NotImplementedException();
            public IFeatureFlag<bool> Remove_ClaimIQ_mock_logic_CLA_758 => throw new NotImplementedException();
            public IFeatureFlag<bool> Expose_Dataflow_Metadata_CLA_2146 => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3329_Expose_HR_Category => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA1656_DataFlowEdit_ViewEditPage => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA1656_DataFlowEdit_SubmitEditPage => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3241_DisableDfsDropLocation => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3497_UniqueLivySessionName => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA2838_DSC_ANOUNCEMENTS => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3550_DATA_INVENTORY_NEW_COLUMNS => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3637_EXPOSE_INV_CATEGORY => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3553_SchemaSearch => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3819_EgressEdgeMigration => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3882_DSC_NOTIFICATION_SUBCATEGORY => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA4049_ALLOW_S3_FILES_DELETE => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4152_UploadFileFromUI => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA1130_SHOW_ALTERNATE_EMAIL => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4310_UseHttpClient => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<string> CLA4260_QuartermasterNamedEnvironmentTypeFilter => new MockStringFeatureFlag("");
            public IFeatureFlag<bool> CLA3756_UpdateSearchPages => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4258_DefaultProdSearchFilter => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA3878_ManageSchemasAccordion => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4411_Goldeneye_Consume_NP_Topics => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA1797_DatasetSchemaMigration => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4925_ParquetFileType => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4912_API => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA5024_PublishReprocessingEvents => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4993_JSMTicketProvider => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4789_ImprovedSearchCapability => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA3214_VariantDataType => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA5211_SendNewSnowflakeEvents => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4553_PlatformActivity => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA5112_PlatformActivity_TotalFiles_ViewPage => new MockBooleanFeatureFlag(true);
            public IFeatureFlag<bool> CLA4870_DSCAssistance => new MockBooleanFeatureFlag(true);
        }
    }
}
