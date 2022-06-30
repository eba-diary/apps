using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Registry;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;
using Sentry.data.Infrastructure.PollyPolicies;
using System.Threading;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Sentry.data.Infrastructure.Tests
{ 
    [TestClass]
    public class ConfluentConnectorProviderTests
    {
        [TestMethod]
        public async Task ConfluentConnectorProvider_GetS3ConnectorsStatus_ReturnCorrectConnectorStatus()
        {
            //Arrange
            const string CONNECTOR_NAME = "S3_ICCM_TESTV2_CCIS_CLAIMANT_02";

            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            Mock<ConfluentConnectorProviderPolicy> pollyPolicyLivy = new Mock<ConfluentConnectorProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            /*Setup provider*/
            HttpResponseMessage httpResponse1 = new HttpResponseMessage(HttpStatusCode.OK);
            JObject connectorStatusJObject = GetData("ConfluentConnector_Status.json");
            string connectorStatusStr = connectorStatusJObject.ToString();
            httpResponse1.Content = new StringContent(connectorStatusStr);

            var StubIHttpClientProvider = new Mock<IHttpClientProvider>();
            StubIHttpClientProvider.Setup(a => a.GetAsync($"/connectors/{CONNECTOR_NAME}/status"))
                .ReturnsAsync(httpResponse1);

            //Setup provider
            ConfluentConnectorProvider providerA = new ConfluentConnectorProvider(StubIHttpClientProvider.Object, policyRegistry);

            //Act
            JObject connectorJObject = await providerA.GetS3ConnectorStatus(CONNECTOR_NAME);

            //Assert
            Assert.AreEqual(CONNECTOR_NAME, connectorJObject["name"]);
        }

        [TestMethod]
        public async Task ConfluentConnectorProvider_GetS3ConnectorsConfig_ReturnCorrectConnectorConfig()
        {
            //Arrange
            const string CONNECTOR_NAME = "S3_ICCM_TESTV2_CCIS_CLAIMANT_02";

            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            Mock<ConfluentConnectorProviderPolicy> pollyPolicyLivy = new Mock<ConfluentConnectorProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            /*Setup provider*/
            HttpResponseMessage httpResponse1 = new HttpResponseMessage(HttpStatusCode.OK);
            JObject connectorStatusJObject = GetData("ConfluentConnector_Config.json");
            string connectorStatusStr = connectorStatusJObject.ToString();
            httpResponse1.Content = new StringContent(connectorStatusStr);

            var StubIHttpClientProvider = new Mock<IHttpClientProvider>();
            StubIHttpClientProvider.Setup(a => a.GetAsync($"/connectors/{CONNECTOR_NAME}/config"))
                .ReturnsAsync(httpResponse1);

            //Setup provider
            ConfluentConnectorProvider providerA = new ConfluentConnectorProvider(StubIHttpClientProvider.Object, policyRegistry);

            //Act
            JObject connectorJObject = await providerA.GetS3ConnectorConfig(CONNECTOR_NAME);

            //Assert
            Assert.AreEqual(CONNECTOR_NAME, connectorJObject["name"]);
        }

        [TestMethod]
        public async Task ConfluentConnectorProvider_GetS3Connectors_ReturnListOf23DTOS()
        {
            //Arrange
            /*Setup Polly Policy*/
            var policyRegistry = new PolicyRegistry();
            Mock<ConfluentConnectorProviderPolicy> pollyPolicyLivy = new Mock<ConfluentConnectorProviderPolicy>(policyRegistry);
            pollyPolicyLivy.Object.Register();

            /*Setup provider*/
            HttpResponseMessage httpResponse1 = new HttpResponseMessage(HttpStatusCode.OK);
            JObject connectorJObjects = GetData("ConfluentConnectors.json");
            string connectorStr = connectorJObjects.ToString();
            httpResponse1.Content = new StringContent(connectorStr);

            HttpResponseMessage httpResponse2 = new HttpResponseMessage(HttpStatusCode.OK);
            JObject connectorNameJObjects = GetData("ConfluentConnectors_Names.json");
            string connectorNamesStr = connectorNameJObjects["connectorNames"].ToString();
            httpResponse2.Content = new StringContent(connectorNamesStr);



            var StubIHttpClientProvider = new Mock<IHttpClientProvider>();
            StubIHttpClientProvider.Setup(a => a.GetAsync("/connectors?expand=status&expand=info"))
                .ReturnsAsync(httpResponse1);
            StubIHttpClientProvider.Setup(a => a.GetAsync("/connectors"))
                .ReturnsAsync(httpResponse2);

            //Setup provider
            ConfluentConnectorProvider providerA = new ConfluentConnectorProvider(StubIHttpClientProvider.Object, policyRegistry);

            //Act
            /*The call for S3 Connectors should only bring back connectors with a connector.class value of io.confluent.connect.s3.S3SinkConnector */
            List<ConnectorRootDto> rootDtos = await providerA.GetS3Connectors();
            
            //Assert
            Assert.AreEqual(23, rootDtos.Count);
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
