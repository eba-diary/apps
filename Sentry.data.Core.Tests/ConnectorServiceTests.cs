using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Sentry.data.Core.DependencyInjection;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class ConnectorServiceTests : DomainServiceUnitTest<ConnectorService>
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            DomainServiceTestInitialize();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TestCleanup();
        }

        [TestMethod]
        public async Task GetS3ConnectorConfigJSONAsync_RemoveS3Password()
        {
            //Arrange

            Mock<IKafkaConnectorProvider> mockProvider = new Mock<IKafkaConnectorProvider>();

            JObject connectorStatusJObject = GetData("ConfluentConnector_Config.json");

            mockProvider.Setup(x=>x.GetS3ConnectorConfigAsync(It.IsAny<string>())).ReturnsAsync(connectorStatusJObject);

            ConnectorService mockService = new ConnectorService(mockProvider.Object, TestDependencies);

            //Act
            JObject configJObj = await mockService.GetS3ConnectorConfigJSONAsync(It.IsAny<string>());

            //Assert
            Assert.AreEqual(false, configJObj.ContainsKey("s3.proxy.password"));
        }

        [TestMethod]
        public async Task CreateS3ConnectorSuccess()
        {
            //Arrange
            Mock<IKafkaConnectorProvider> mockProvider = new Mock<IKafkaConnectorProvider>();
            
            HttpResponseMessage httpResponse = new HttpResponseMessage() 
            { 
                StatusCode = System.Net.HttpStatusCode.Created,
                ReasonPhrase = "Created",
            };
            mockProvider.Setup(x => x.CreateS3SinkConnectorAsync(It.IsAny<string>())).ReturnsAsync(httpResponse);

            ConnectorService mockService = new ConnectorService(mockProvider.Object, TestDependencies);


            ConnectorCreateRequestDto requestDto = MockClasses.MockConnectorCreateRequestDto();

            //Act
            ConnectorCreateResponseDto responseDto= await mockService.CreateS3SinkConnectorAsync(requestDto);

            //Assert
            Assert.AreEqual(true, responseDto.SuccessStatusCode);
        }

        [TestMethod]
        public async Task CreateS3ConnectorFailure()
        {
            //Arrange
            Mock<IKafkaConnectorProvider> mockProvider = new Mock<IKafkaConnectorProvider>();

            HttpResponseMessage httpResponse = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                ReasonPhrase = "Not Found",
            };
            mockProvider.Setup(x => x.CreateS3SinkConnectorAsync(It.IsAny<string>())).ReturnsAsync(httpResponse);

            ConnectorService mockService = new ConnectorService(mockProvider.Object, TestDependencies);


            ConnectorCreateRequestDto requestDto = MockClasses.MockConnectorCreateRequestDto();

            //Act
            ConnectorCreateResponseDto responseDto = await mockService.CreateS3SinkConnectorAsync(requestDto);

            //Assert
            Assert.AreEqual(false, responseDto.SuccessStatusCode);
        }

        [TestMethod]
        public async Task CreateS3ConnectorUnknownStatusStillFails()
        {
            //Arrange
            Mock<IKafkaConnectorProvider> mockProvider = new Mock<IKafkaConnectorProvider>();

            HttpResponseMessage httpResponse = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadGateway,
                ReasonPhrase = "Not Found",
            };
            mockProvider.Setup(x => x.CreateS3SinkConnectorAsync(It.IsAny<string>())).ReturnsAsync(httpResponse);

            ConnectorService mockService = new ConnectorService(mockProvider.Object, TestDependencies);


            ConnectorCreateRequestDto requestDto = MockClasses.MockConnectorCreateRequestDto();

            //Act
            ConnectorCreateResponseDto responseDto = await mockService.CreateS3SinkConnectorAsync(requestDto);

            //Assert
            Assert.AreEqual(false, responseDto.SuccessStatusCode);
        }
    }
}
