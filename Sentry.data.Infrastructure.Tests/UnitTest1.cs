using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Schema.Elastic;
using Sentry.data.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            Bootstrapper.Init();

            IElasticContext elasticContext = Bootstrapper.Container.GetInstance<IElasticContext>();

            await elasticContext.DoThing<ElasticSchemaField>();
        }
    }
}
