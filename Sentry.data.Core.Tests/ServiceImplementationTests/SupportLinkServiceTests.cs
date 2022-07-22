using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests.ServiceImplementationTests
{
    [TestClass]
    public class SupportLinkServiceTests : BaseCoreUnitTest
    {
        [TestMethod]
        public void TestAddSupportLink()
        {
            var context = new Mock<IDatasetContext>();
        }


        [TestMethod]
        public void TestRemoveSupportLink()
        {
            var context = new Mock<IDatasetContext>();
        }
    }
}
