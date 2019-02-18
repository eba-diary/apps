using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Mocks;
using StructureMap;

namespace Sentry.data.Core.Tests
{
    public class BaseCoreUnitTest
    {
        protected static IContainer _container;


        protected ISecurityService _securityService;
        protected IDatasetContext _datasetContext;
        protected IHpsmProvider _hpsmProvider;



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
            registry.For<IHpsmProvider>().Use(() => MockRepository.GenerateStub<IHpsmProvider>());
            registry.For<IDatasetContext>().Use(() => MockRepository.GenerateStub<IDatasetContext>());

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
