﻿using Newtonsoft.Json.Linq;
using Rhino.Mocks;
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


        protected ISecurityService _securityService;
        protected IDatasetContext _datasetContext;

        public virtual void TestInitialize()
        {
            StructureMap.Registry registry = new StructureMap.Registry();

            //this should can all core assembly for implementations
            registry.Scan((scanner) =>
            {
                scanner.AssemblyContainingType<ISecurityService>();
                scanner.WithDefaultConventions();
            });

            //add in the infrastructure implementations using MockRepository so we don't actually initalize contexts or services.
            registry.For<IDatasetContext>().Use(() => MockRepository.GenerateStub<IDatasetContext>());
            registry.For<IBaseTicketProvider>().Use(() => MockRepository.GenerateStub<ICherwellProvider>());
            registry.For<IDataFeatures>().Use(new MockDataFeatures());
            registry.For<IInevService>().Use(() => MockRepository.GenerateStub<IInevService>());

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

            public IFeatureFlag<bool> CLA3240_UseDropLocationV2 { get; } = new MockBooleanFeatureFlag(false);

            public IFeatureFlag<bool> CLA3241_DisableDfsDropLocation => throw new NotImplementedException();

            public IFeatureFlag<bool> CLA3332_ConsolidatedDataFlows { get; } = new MockBooleanFeatureFlag(false);

            public IFeatureFlag<bool> CLA3048_StandardizeOnUTCTime => throw new NotImplementedException();

            public IFeatureFlag<bool> CLA3497_UniqueLivySessionName => throw new NotImplementedException();

            public IFeatureFlag<bool> CLA2838_DSC_ANOUNCEMENTS => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3550_DATA_INVENTORY_NEW_COLUMNS => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3541_Dataset_Details_Tabs => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3605_AllowSchemaParquetUpdate => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3637_EXPOSE_INV_CATEGORY => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3553_SchemaSearch => throw new NotImplementedException();

            public IFeatureFlag<bool> CLA3861_RefactorGetUserSecurity { get; } = new MockBooleanFeatureFlag(true);

            public IFeatureFlag<bool> CLA3819_EgressEdgeMigration => throw new NotImplementedException();
            public IFeatureFlag<bool> CLA3882_DSC_NOTIFICATION_SUBCATEGORY => throw new NotImplementedException();

            public IFeatureFlag<bool> CLA3718_Authorization { get; } = new MockBooleanFeatureFlag(true);
        }
    }
}
