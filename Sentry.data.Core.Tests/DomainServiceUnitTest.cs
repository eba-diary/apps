using Moq;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void DomainServiceTestInitalize(MockBehavior mockBehavior = MockBehavior.Strict)
        {
            TestInitialize(mockBehavior);

            _testDependencies = new DomainServiceCommonDependency<T>();
            _testDependencies._logger = new MockLoggingService<T>();
            _testDependencies._dataFeatures = _container.GetInstance<IDataFeatures>();
        }
    }
}
