using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;


namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DataFlowProviderTests : BaseCoreUnitTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            TestInitialize();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TestCleanup();
        }        
    }
}
