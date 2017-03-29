using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using StructureMap;

namespace Sentry.data
{
    public class StructureMapWebApiDependencyScope : IDependencyScope
    {
        private IContainer _container;

        public StructureMapWebApiDependencyScope(IContainer container)
        {
            _container = container;
        }

        public Object GetService(Type serviceType)
        {
            if (serviceType.IsAbstract || serviceType.IsInterface)
            {
                return _container.TryGetInstance(serviceType);
            }
            else
            {
                return _container.GetInstance(serviceType);
            }
        }

        public IEnumerable<Object> GetServices(Type serviceType)
        {
            return _container.GetAllInstances(serviceType).Cast<Object>();
        }

        private Boolean disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(Boolean disposing)
        {
            if (! this.disposedValue)
            {
                if (disposing)
                {
                    _container.Dispose();
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
