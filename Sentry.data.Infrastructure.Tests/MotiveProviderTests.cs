using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class MotiveProviderTests : BaseInfrastructureUnitTest
    {
        [TestMethod]
        public async Task MotiveOnBoardingAsync_Expected()
        {
            DateTime originalExp = DateTime.UtcNow.AddMinutes(-1);
            DataSourceToken token = new DataSourceToken
            {
                TokenName = "OriginalName",
                CurrentToken = "CurrentToken",
                CurrentTokenExp = originalExp,
                RefreshToken = "EncryptedRefreshToken",
                TokenUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=refreshtoken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=clientid&client_secret=clientsecret"
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(GetDataString("Motive_Companies.json")),
                StatusCode = HttpStatusCode.OK
            };

            string requestUrl = "https://api.keeptruckin.com/v1/companies";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                GrantType = OAuthGrantType.RefreshToken,
                ClientId = "ClientId",
                ClientPrivateId = "EncryptedPrivateId"
            };

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();

            Mock<IAuthorizationProvider> authProvider = repository.Create<IAuthorizationProvider>();
            authProvider.Setup(x => x.GetOAuthAccessToken(source, token)).Returns("ACCESS_TOKEN");

            datasetContext.Setup(x => x.SaveChanges(true));

            MotiveProvider motiveProvider = new MotiveProvider(httpClient, datasetContext.Object, authProvider.Object, null, null);

            await motiveProvider.MotiveOnboardingAsync(source, token, 999);

            Assert.AreEqual("COMPANY NAME", token.TokenName);
            
            repository.VerifyAll();
        }

        [TestMethod]
        public async Task MotiveOnBoardingAsync_Unexpected()
        {
            DateTime originalExp = DateTime.UtcNow.AddMinutes(-1);
            DataSourceToken token = new DataSourceToken
            {
                TokenName = "OriginalName",
                CurrentToken = "CurrentToken",
                CurrentTokenExp = originalExp,
                RefreshToken = "EncryptedRefreshToken",
                TokenUrl = "https://keeptruckin.com/oauth/token?grant_type=refresh_token&refresh_token=refreshtoken&redirect_uri=https://webhook.site/27091c3b-f9d0-42a2-a0d0-51b5134ac128&client_id=clientid&client_secret=clientsecret"
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(GetDataString("Motive_Companies_Unexpected.json")),
                StatusCode = HttpStatusCode.OK
            };

            string requestUrl = "https://api.keeptruckin.com/v1/companies";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            HTTPSSource source = new HTTPSSource
            {
                IVKey = "IVKey",
                GrantType = OAuthGrantType.RefreshToken,
                ClientId = "ClientId",
                ClientPrivateId = "EncryptedPrivateId"
            };


            Mock<IAuthorizationProvider> authProvider = repository.Create<IAuthorizationProvider>();
            authProvider.Setup(x => x.GetOAuthAccessToken(source, token)).Returns("ACCESS_TOKEN");

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.SaveChanges(true));

            MotiveProvider motiveProvider = new MotiveProvider(httpClient, datasetContext.Object, authProvider.Object, null, null);

            await motiveProvider.MotiveOnboardingAsync(source, token, 999);

            Assert.AreEqual("OriginalName", token.TokenName);

            repository.VerifyAll();
        }

        [TestMethod]
        public void MotiveTokenBackfill_Expected()
        {
            DataSourceToken token0 = new DataSourceToken { Id = 2, Enabled = false };
            DataSourceToken token = new DataSourceToken { Id = 3, Enabled = true };
            DataSourceToken token2 = new DataSourceToken { Id = 4, Enabled = true };
            DataSourceToken token3 = new DataSourceToken { Id = 5, Enabled = false };
            DataSourceToken token4 = new DataSourceToken { Id = 6, Enabled = true };
            DataSourceToken token5 = new DataSourceToken { Id = 7, Enabled = true };
            DataSourceToken token6 = new DataSourceToken { Id = 8, Enabled = false };
            DataSourceToken backfillToken = new DataSourceToken { Id = 9, Enabled = false, BackfillComplete = false };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                AllTokens = new List<DataSourceToken> { token0, token, token2, token3, token4, token5, token6, backfillToken }
            };
            Dataset mockMotiveDataset = new Dataset { DatasetId = 7015 };
            List<DatasetFileConfig> mockFileConfigList = new List<DatasetFileConfig>
            {
                new DatasetFileConfig
                {
                    ParentDataset = mockMotiveDataset,
                    Schema = new FileSchema
                    {
                        SchemaId = 23
                    }
                },
                new DatasetFileConfig
                {
                    ParentDataset = mockMotiveDataset,
                    Schema = new FileSchema
                    {
                        SchemaId = 25
                    }
                },
                new DatasetFileConfig
                {
                    ParentDataset = mockMotiveDataset,
                    Schema = new FileSchema
                    {
                        SchemaId = 26
                    }
                },
            };

            List<int> mockSchemaIdList = new List<int> { 23, 25, 26 };

            RetrieverJob job1 = new RetrieverJob
            {
                FileSchema = new FileSchema
                {
                    SchemaId = 23,
                    CreateCurrentView = false
                },
                IsEnabled = true,
                RequestVariables =  new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "dateValue",
                        VariableValue = "2023-03-04"
                    }
                },
                DataFlow = new DataFlow
                {
                    Name = "Unit Test 1"
                }
            };

            RetrieverJob job2 = new RetrieverJob
            {
                FileSchema = new FileSchema
                {
                    SchemaId = 25,
                    CreateCurrentView = false
                },
                IsEnabled = true,
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "dateValue",
                        VariableValue = "2023-03-08"
                    }
                },
                DataFlow = new DataFlow
                {
                    Name = "Unit Test 2"
                }
            };

            RetrieverJob job3 = new RetrieverJob
            {
                FileSchema = new FileSchema
                {
                    SchemaId = 26,
                    CreateCurrentView = false
                },
                IsEnabled = true,
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "dateValue",
                        VariableValue = "2023-03-10"
                    }
                },
                DataFlow = new DataFlow
                {
                    Name = "Unit Test 3"
                }
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.GetById<HTTPSSource>(It.IsAny<int>())).Returns(dataSource);

            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(mockFileConfigList.AsQueryable());

            datasetContext.SetupGet(x => x.Jobs).Returns(new List<RetrieverJob> { job1, job2, job3 }.AsQueryable());

            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IBaseJobProvider> jobProvider = new Mock<IBaseJobProvider>();
            jobProvider.Setup(x => x.Execute(It.IsAny<RetrieverJob>()));

            MotiveProvider motiveProvider = new MotiveProvider(null, datasetContext.Object, null, null, jobProvider.Object);

            var result = motiveProvider.MotiveTokenBackfill(backfillToken);

            //backfill marked complete
            Assert.IsTrue(backfillToken.BackfillComplete);

            //all token state is back  to what it should be
            Assert.IsFalse(token0.Enabled);
            Assert.IsTrue(token.Enabled);
            Assert.IsTrue(token2.Enabled);
            Assert.IsFalse(token3.Enabled);
            Assert.IsTrue(token4.Enabled);
            Assert.IsTrue(token5.Enabled);
            Assert.IsFalse(token6.Enabled);
            Assert.IsTrue(backfillToken.Enabled);

            //jobs set back to right place 
            Assert.AreEqual(job1.RequestVariables.First(rv => rv.VariableName == "dateValue").VariableValue, "2023-03-04");
            Assert.AreEqual(job2.RequestVariables.First(rv => rv.VariableName == "dateValue").VariableValue, "2023-03-08");
            Assert.AreEqual(job3.RequestVariables.First(rv => rv.VariableName == "dateValue").VariableValue, "2023-03-10");

            jobProvider.Verify(jp => jp.Execute(It.IsAny<RetrieverJob>()), Times.Exactly(3));

            repository.VerifyAll();
        }

        [TestMethod]
        public void MotiveTokenBackfill_Unexpected()
        {
            DataSourceToken token0 = new DataSourceToken { Id = 2, Enabled = false };
            DataSourceToken token = new DataSourceToken { Id = 3, Enabled = true };
            DataSourceToken token2 = new DataSourceToken { Id = 4, Enabled = true };
            DataSourceToken token3 = new DataSourceToken { Id = 5, Enabled = false };
            DataSourceToken token4 = new DataSourceToken { Id = 6, Enabled = true };
            DataSourceToken token5 = new DataSourceToken { Id = 7, Enabled = true };
            DataSourceToken token6 = new DataSourceToken { Id = 8, Enabled = false };
            DataSourceToken backfillToken = new DataSourceToken { Id = 9, Enabled = false, BackfillComplete = false };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                AllTokens = new List<DataSourceToken> { token0, token, token2, token3, token4, token5, token6, backfillToken }
            };
            Dataset mockMotiveDataset = new Dataset { DatasetId = 7015 };
            List<DatasetFileConfig> mockFileConfigList = new List<DatasetFileConfig>
            {
                new DatasetFileConfig
                {
                    ParentDataset = mockMotiveDataset,
                    Schema = new FileSchema
                    {
                        SchemaId = 23
                    }
                },
                new DatasetFileConfig
                {
                    ParentDataset = mockMotiveDataset,
                    Schema = new FileSchema
                    {
                        SchemaId = 25
                    }
                },
                new DatasetFileConfig
                {
                    ParentDataset = mockMotiveDataset,
                    Schema = new FileSchema
                    {
                        SchemaId = 26
                    }
                },
            };

            List<int> mockSchemaIdList = new List<int> { 23, 25, 26 };

            RetrieverJob job1 = new RetrieverJob
            {
                FileSchema = new FileSchema
                {
                    SchemaId = 23,
                    CreateCurrentView = false
                },
                IsEnabled = true,
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "dateValue",
                        VariableValue = "2023-03-04"
                    }
                },
                DataFlow = new DataFlow
                {
                    Name = "Unit Test 1"
                }
            };

            RetrieverJob job2 = new RetrieverJob
            {
                FileSchema = new FileSchema
                {
                    SchemaId = 25,
                    CreateCurrentView = false
                },
                IsEnabled = true,
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "startValue",
                        VariableValue = "2023-03-08"
                    }
                },
                DataFlow = new DataFlow
                {
                    Name = "Unit Test 2"
                }
            };

            RetrieverJob job3 = new RetrieverJob
            {
                FileSchema = new FileSchema
                {
                    SchemaId = 26,
                    CreateCurrentView = false
                },
                IsEnabled = true,
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "endValue",
                        VariableValue = "2023-03-10"
                    }
                },
                DataFlow = new DataFlow
                {
                    Name = "Unit Test 3"
                }
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.GetById<HTTPSSource>(It.IsAny<int>())).Returns(dataSource);

            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(mockFileConfigList.AsQueryable());

            datasetContext.SetupGet(x => x.Jobs).Returns(new List<RetrieverJob> { job1, job2, job3 }.AsQueryable());

            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IBaseJobProvider> jobProvider = new Mock<IBaseJobProvider>();
            jobProvider.Setup(x => x.Execute(It.IsAny<RetrieverJob>()));

            MotiveProvider motiveProvider = new MotiveProvider(null, datasetContext.Object, null, null, jobProvider.Object);

            var result = motiveProvider.MotiveTokenBackfill(backfillToken);

            //backfill marked complete
            Assert.IsFalse(backfillToken.BackfillComplete);

            //all token state is back  to what it should be
            Assert.IsFalse(token0.Enabled);
            Assert.IsTrue(token.Enabled);
            Assert.IsTrue(token2.Enabled);
            Assert.IsFalse(token3.Enabled);
            Assert.IsTrue(token4.Enabled);
            Assert.IsTrue(token5.Enabled);
            Assert.IsFalse(token6.Enabled);
            Assert.IsTrue(backfillToken.Enabled);

            //jobs set back to right place 
            Assert.AreEqual(job1.RequestVariables.First(rv => rv.VariableName == "dateValue").VariableValue, "2023-03-04");
            Assert.AreEqual(job2.RequestVariables.First(rv => rv.VariableName == "startValue").VariableValue, "2023-03-08");
            Assert.AreEqual(job3.RequestVariables.First(rv => rv.VariableName == "endValue").VariableValue, "2023-03-10");

            jobProvider.Verify(jp => jp.Execute(It.IsAny<RetrieverJob>()), Times.Exactly(1));

            repository.VerifyAll();
        }
    }
}
