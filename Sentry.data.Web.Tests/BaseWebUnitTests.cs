using Moq;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using StructureMap;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    public class BaseWebUnitTests
    {
        protected static IContainer _container;
        protected ISecurityService _securityService;
        protected IDatasetContext _datasetContext;
        protected MockRepository _mockRepository;

        public virtual void TestInitialize()
        {
            StructureMap.Registry registry = new StructureMap.Registry();

            //this should can all core assembly for implementations
            registry.Scan((scanner) =>
            {
                scanner.AssemblyContainingType<ISecurityService>();
                scanner.WithDefaultConventions();
            });

            MockRepository mr = new MockRepository(MockBehavior.Strict);
            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();

            //add in the infrastructure implementations using MockRepository so we don't actually initalize contexts or services.
            registry.For<IDatasetContext>().Use(() => context.Object);
            //set the container
            _container = new StructureMap.Container(registry);

            //Set up very resuable datasetContext objects here if needed.
            context.Setup(s => s.DataFlow).Returns(new List<DataFlow>()
            {
                new DataFlow() { Id = 11, NamedEnvironment = "TEST", NamedEnvironmentType = Core.GlobalEnums.NamedEnvironmentType.NonProd, ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active, SchemaId = 44 },
                new DataFlow() { Id = 22, NamedEnvironment = "TEST", NamedEnvironmentType = Core.GlobalEnums.NamedEnvironmentType.NonProd, ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Deleted, SchemaId = 66 },
                new DataFlow() { Id = 33, NamedEnvironment = "TEST", NamedEnvironmentType = Core.GlobalEnums.NamedEnvironmentType.NonProd, ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Disabled, SchemaId = 88 }
            }.AsQueryable());
        }


        protected void TestCleanup()
        {
            _container.Dispose();
        }
    }
}
