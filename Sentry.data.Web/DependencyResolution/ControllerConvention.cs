using System;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using StructureMap.Pipeline;
using StructureMap.TypeRules;
using System.Web.Mvc;

namespace Sentry.data
{
    public class ControllerConvention : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, Registry registry)
        {
            foreach (Type t in types.AllTypes())
            {
                if (t.CanBeCastTo<Controller>() && ! t.IsAbstract)
                {
                    registry.For(t).LifecycleIs(new UniquePerRequestLifecycle());
                }
            }
        }
    }
}
