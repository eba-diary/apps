using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

namespace Sentry.data
{
    public class StructureMapWebApiDependencyResolver : IDependencyResolver
    {
        private IContainer _rootContainer;

        public StructureMapWebApiDependencyResolver(IContainer rootContainer)
        {
            _rootContainer = rootContainer;
        }

        public IDependencyScope BeginScope()
        {
            return new StructureMapWebApiDependencyScope(_rootContainer.GetNestedContainer());
        }

        public Object GetService(Type serviceType)
        {
            if (serviceType.IsAbstract || serviceType.IsInterface)
            {
                return _rootContainer.TryGetInstance(serviceType);
            }
            else
            {
                return _rootContainer.GetInstance(serviceType);
            }
        }

        public IEnumerable<Object> GetServices(Type serviceType)
        {
            return _rootContainer.GetAllInstances(serviceType).Cast<Object>();
        }

        private Boolean disposedValue;// To detect redundant calls

        // IDisposable
        protected virtual void Dispose(Boolean disposing)
        {
            if (! this.disposedValue)
            {
                if (disposing)
                {
                    _rootContainer.Dispose();
                }
            }
            this.disposedValue = true;
        }


        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
