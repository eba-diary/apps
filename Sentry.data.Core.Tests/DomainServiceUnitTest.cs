using Moq;
using Sentry.data.Core.DependencyInjection;

namespace Sentry.data.Core.Tests
{
    public class DomainServiceUnitTest<T> : BaseCoreUnitTest
    {
        private DomainServiceCommonDependency<T> _testDependencies;

        protected DomainServiceCommonDependency<T> TestDependencies
        {
            get
            {
                return _testDependencies;
            }
        }

        public void DomainServiceTestInitialize(MockBehavior mockBehavior = MockBehavior.Strict)
        {
            TestInitialize(mockBehavior);

            _testDependencies = new DomainServiceCommonDependency<T>(new MockLoggingService<T>(), _container.GetInstance<IDataFeatures>());
        }
    }
}
