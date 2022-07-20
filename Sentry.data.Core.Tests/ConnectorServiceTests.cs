using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class ConnectorServiceTests : BaseCoreUnitTest
    {
        [TestMethod]
        public async Task GetS3ConnectorConfigJSONAsync_RemoveS3Password()
        {
            //Arrange
            Mock<IKafkaConnectorProvider> mockProvider = new Mock<IKafkaConnectorProvider>();

            JObject connectorStatusJObject = GetData("ConfluentConnector_Config.json");

            mockProvider.Setup(x=>x.GetS3ConnectorConfigAsync(It.IsAny<string>())).ReturnsAsync(connectorStatusJObject);

            ConnectorService mockService = new ConnectorService(mockProvider.Object);

            //Act
            JObject configJObj = await mockService.GetS3ConnectorConfigJSONAsync(It.IsAny<string>());

            //Assert
            Assert.AreEqual(false, configJObj.ContainsKey("s3.proxy.password"));
        }

        protected JObject GetData(string fileName)
        {
            using (StreamReader rdr = new StreamReader($@"ExpectedJSON\{fileName}"))
            {
                return JObject.Parse(rdr.ReadToEnd().Replace("\r\n", string.Empty));
            }
        }
    }
}
