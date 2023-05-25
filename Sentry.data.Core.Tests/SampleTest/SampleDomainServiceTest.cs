using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.DomainServices.SampleService;
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace Sentry.data.Core.Tests.SampleTest
{
    [TestClass]
    public class SampleDomainServiceTest : DomainServiceUnitTest<SampleDomainService>
    {
        [TestInitialize]
        public void MyTestInitialization()
        {
            DomainServiceTestInitialize(MockBehavior.Strict);
        }

        /// <summary>
        /// Comment out the ignore if you want to run this example unit test
        /// </summary>
        //[Ignore("This is an example domain service unit test, no need for it to run every build")]
        [TestMethod]
        public void SimpleLogStatement()
        {
            //Arrange
            Dataset dataset = new Dataset()
            {
                DatasetId = 1,
                DatasetName = "MyDataset"
            };

            _datasetContext.Setup(s => s.Datasets).Returns(new List<Dataset>() { dataset }.AsQueryable());

            SampleDomainService sampleDomainService = new SampleDomainService(_datasetContext.Object, TestDependencies);
            //Act

            string result = sampleDomainService.SimpleLogStatement();

            //Assert
            _mockRepository.VerifyAll();
            Assert.AreEqual("MyDataset", result);

        }
    }
}
