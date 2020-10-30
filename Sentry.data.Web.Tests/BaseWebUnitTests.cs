using Rhino.Mocks;
using Sentry.data.Core;
using StructureMap;

namespace Sentry.data.Web.Tests
{
    public class BaseWebUnitTests
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
            //set the container
            _container = new StructureMap.Container(registry);

            //Set up very resuable datasetContext objects here if needed.

        }


        protected void TestCleanup()
        {
            _container.Dispose();
        }
    }
}
