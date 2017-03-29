using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StructureMap;
using System.Web;

namespace Sentry.data
{
    // MVC and WebApi use two slightly different approaches for resolving dependencies.
    // MVC doesn't use "scoped" containers, so it's up to the StructureMapMvcDependencyResolver
    // to track one nested container per http request.  It stores the nested container inside
    // the HttpContext.Items collection, which effectively makes it scoped to each request.
    // Global.asax EndRequest calls the DisposeNestedContainer method, which disposes
    // the nested container and removes it from HttpContext.  This ensures that
    // any dependencies that were created within this nested container (such as 
    // DomainContexts, NHibernate Sessions, etc) are properly closed and released.

    public class StructureMapMvcDependencyResolver : IDependencyResolver
    {
        private const string NESTED_CONTAINER_KEY = "Nested.Container.Key";

        public IContainer RootContainer { get; set; }

        public StructureMapMvcDependencyResolver(IContainer container)
        {
            this.RootContainer = container;
        }

        public IContainer CurrentNestedContainer
        {
            get
            {
                if (HttpContext.Items[NESTED_CONTAINER_KEY] == null)
                {
                    HttpContext.Items[NESTED_CONTAINER_KEY] = RootContainer.GetNestedContainer();
                }
                return (IContainer)HttpContext.Items[NESTED_CONTAINER_KEY];
            }
        }

        public Object GetService(Type serviceType)
        {
            if (serviceType.IsAbstract || serviceType.IsInterface)
            {
                return CurrentNestedContainer.TryGetInstance(serviceType);
            }
            else
            {
                return CurrentNestedContainer.GetInstance(serviceType);
            }
        }

        public IEnumerable<Object> GetServices(Type serviceType)
        {
            return CurrentNestedContainer.GetAllInstances(serviceType).Cast<Object>();
        }

        private HttpContextBase HttpContext
        {
            get
            {
                HttpContextBase ctx = RootContainer.TryGetInstance<HttpContextBase>();
                // ?? is a Null-Coalescing operator.
                return ctx ?? new HttpContextWrapper(System.Web.HttpContext.Current);
            }
        }

        public void DisposeNestedContainer()
        {
            if (HttpContext.Items[NESTED_CONTAINER_KEY] != null)
            {
                ((IContainer)HttpContext.Items[NESTED_CONTAINER_KEY]).Dispose();
                HttpContext.Items.Remove(NESTED_CONTAINER_KEY);
            }
        }
    }
}
