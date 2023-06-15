using Hangfire;
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

            MotiveProvider motiveProvider = new MotiveProvider(httpClient, datasetContext.Object, authProvider.Object, null, null, null);

            await motiveProvider.MotiveOnboardingAsync(source, token);

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

            MotiveProvider motiveProvider = new MotiveProvider(httpClient, datasetContext.Object, authProvider.Object, null, null, null);

            await motiveProvider.MotiveOnboardingAsync(source, token);

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
                    Name = "Unit Test 1",
                    Id = 1
                },
                JobOptions = new RetrieverJobOptions
                {
                    TargetFileName = "job1",
                    HttpOptions = new RetrieverJobOptions.HttpsOptions
                    {
                        PagingType = PagingType.None
                    }
                },
                RelativeUri = "companies?~[]~"
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
                    Name = "Unit Test 2",
                    Id = 2
                },
                JobOptions = new RetrieverJobOptions
                {
                    TargetFileName = "job2",
                    HttpOptions = new RetrieverJobOptions.HttpsOptions
                    {
                        PagingType = PagingType.None
                    }
                },
                RelativeUri = "companies?~[]~"
            };

            var dataflow1 = new DataFlowStep
            {
                Id = 1,
                DataFlow = new DataFlow
                {
                    Id = 1
                },
                DataAction_Type_Id = DataActionType.ProducerS3Drop,
                TriggerKey = "TriggerKey1",
                TriggerBucket = "TriggerBucket1"
            };
            
            var dataflow2 = new DataFlowStep
            {
                Id = 2,
                DataFlow = new DataFlow
                {
                    Id = 2
                },
                DataAction_Type_Id = DataActionType.ProducerS3Drop,
                TriggerKey = "TriggerKey2",
                TriggerBucket = "TriggerBucket2"
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.GetById<HTTPSSource>(It.IsAny<int>())).Returns(dataSource);

            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(mockFileConfigList.AsQueryable());

            datasetContext.SetupGet(x => x.Jobs).Returns(new List<RetrieverJob> { job1, job2 }.AsQueryable());
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(new List<DataFlowStep> { dataflow1, dataflow2 }.AsQueryable());

            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IAuthorizationProvider> mockAuthProvider = new Mock<IAuthorizationProvider>();
            mockAuthProvider.Setup(x => x.GetOAuthAccessToken(It.IsAny<HTTPSSource>(), It.IsAny<DataSourceToken>())).Returns("OAuthToken");

            Mock<Stream> stream = repository.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.SetupSequence(x => x.Length).Returns(0).Returns(1);

            Mock<IFileProvider> fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(x => x.GetFileStream(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>())).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteDirectory(It.IsAny<string>()));
            fileProvider.Setup(x => x.CreateDirectory(It.IsAny<string>()));

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = CreateResponseMessage("[]");  
            HttpResponseMessage responseMessage2 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage3 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage4 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage5 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage6 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage7 = CreateResponseMessage("[]");


            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.IsAny<HttpRequestMessage>(),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage).ReturnsAsync(responseMessage2).ReturnsAsync(responseMessage3).ReturnsAsync(responseMessage4)
                                                                                                              .ReturnsAsync(responseMessage5).ReturnsAsync(responseMessage6).ReturnsAsync(responseMessage7);
            
            Mock<HttpMessageHandler> httpMessageHandler2 = repository.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage8 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage9 = CreateResponseMessage("[]");  
            HttpResponseMessage responseMessage10 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage11 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage12 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage13 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage14 = CreateResponseMessage("[]");



            httpMessageHandler2.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.IsAny<HttpRequestMessage>(),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage8).ReturnsAsync(responseMessage9).ReturnsAsync(responseMessage10).ReturnsAsync(responseMessage11)
                                                                                                              .ReturnsAsync(responseMessage12).ReturnsAsync(responseMessage13).ReturnsAsync(responseMessage14);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));
            httpMessageHandler2.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));


            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);
            HttpClient httpClient2 = new HttpClient(httpMessageHandler2.Object, true);

            Mock<IHttpClientGenerator> generator = repository.Create<IHttpClientGenerator>();
            generator.SetupSequence(x => x.GenerateHttpClient(It.IsAny<string>())).Returns(httpClient).Returns(httpClient2);

            Mock<IS3ServiceProvider> s3provider = new Mock<IS3ServiceProvider>();
            s3provider.Setup(x => x.UploadDataFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>())).Returns("");

            PagingHttpsJobProvider pagingProvider = new PagingHttpsJobProvider(datasetContext.Object, s3provider.Object, mockAuthProvider.Object, generator.Object, fileProvider.Object);

            Mock<IBackgroundJobClient> backgroundClient = new Mock<IBackgroundJobClient>();

            MotiveProvider motiveProvider = new MotiveProvider(null, datasetContext.Object, null, null, pagingProvider, backgroundClient.Object);

            motiveProvider.MotiveTokenBackfill(backfillToken, DateTime.Today.AddDays(-7));

            //backfill marked complete
            Assert.IsTrue(dataSource.AllTokens.First(t => t.Id == backfillToken.Id).BackfillComplete);

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
            Assert.AreEqual("2023-03-04", job1.RequestVariables.First(rv => rv.VariableName == "dateValue").VariableValue);
            Assert.AreEqual("2023-03-08", job2.RequestVariables.First(rv => rv.VariableName == "dateValue").VariableValue);

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
                    Name = "Unit Test 1",
                    Id = 1
                },
                JobOptions = new RetrieverJobOptions
                {
                    TargetFileName = "job1",
                    HttpOptions = new RetrieverJobOptions.HttpsOptions
                    {
                        PagingType = PagingType.None
                    }
                },
                RelativeUri = "companies?~[]~"
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
                    Name = "Unit Test 2",
                    Id = 2
                },
                JobOptions = new RetrieverJobOptions
                {
                    TargetFileName = "job2",
                    HttpOptions = new RetrieverJobOptions.HttpsOptions
                    {
                        PagingType = PagingType.None
                    }
                },
                RelativeUri = "companies?~[]~"
            };

            var dataflow1 = new DataFlowStep
            {
                Id = 1,
                DataFlow = new DataFlow
                {
                    Id = 1
                },
                DataAction_Type_Id = DataActionType.ProducerS3Drop,
                TriggerKey = "TriggerKey1",
                TriggerBucket = "TriggerBucket1"
            };

            var dataflow2 = new DataFlowStep
            {
                Id = 2,
                DataFlow = new DataFlow
                {
                    Id = 2
                },
                DataAction_Type_Id = DataActionType.ProducerS3Drop,
                TriggerKey = "TriggerKey2",
                TriggerBucket = "TriggerBucket2"
            };

            MockRepository repository = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = repository.Create<IDatasetContext>();
            datasetContext.Setup(x => x.GetById<HTTPSSource>(It.IsAny<int>())).Returns(dataSource);

            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(mockFileConfigList.AsQueryable());

            datasetContext.SetupGet(x => x.Jobs).Returns(new List<RetrieverJob> { job1, job2 }.AsQueryable());
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(new List<DataFlowStep> { dataflow1, dataflow2 }.AsQueryable());

            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IAuthorizationProvider> mockAuthProvider = new Mock<IAuthorizationProvider>();
            mockAuthProvider.Setup(x => x.GetOAuthAccessToken(It.IsAny<HTTPSSource>(), It.IsAny<DataSourceToken>())).Returns("OAuthToken");

            Mock<Stream> stream = repository.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.SetupSequence(x => x.Length).Returns(0).Returns(1);

            Mock<IFileProvider> fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(x => x.GetFileStream(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>())).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteDirectory(It.IsAny<string>()));
            fileProvider.Setup(x => x.CreateDirectory(It.IsAny<string>()));

            Mock<HttpMessageHandler> httpMessageHandler = repository.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage2 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage3 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage4 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage5 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage6 = CreateResponseMessage("[]");
            HttpResponseMessage responseMessage7 = CreateResponseMessage("[]");


            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.IsAny<HttpRequestMessage>(),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage).ReturnsAsync(responseMessage2).ReturnsAsync(responseMessage3).ReturnsAsync(responseMessage4)
                                                                                                              .ReturnsAsync(responseMessage5).ReturnsAsync(responseMessage6).ReturnsAsync(responseMessage7);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));


            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repository.Create<IHttpClientGenerator>();
            generator.SetupSequence(x => x.GenerateHttpClient(It.IsAny<string>())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3provider = new Mock<IS3ServiceProvider>();
            s3provider.Setup(x => x.UploadDataFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>())).Returns("");

            PagingHttpsJobProvider pagingProvider = new PagingHttpsJobProvider(datasetContext.Object, s3provider.Object, mockAuthProvider.Object, generator.Object, fileProvider.Object);

            Mock<IBackgroundJobClient> backgroundClient = new Mock<IBackgroundJobClient>();

            MotiveProvider motiveProvider = new MotiveProvider(null, datasetContext.Object, null, null, pagingProvider, backgroundClient.Object);

            motiveProvider.MotiveTokenBackfill(backfillToken, DateTime.Today.AddDays(-7));

            //backfill marked complete
            Assert.IsFalse(dataSource.AllTokens.First(t => t.Id == backfillToken.Id).BackfillComplete);

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
            Assert.AreEqual("2023-03-04", job1.RequestVariables.First(rv => rv.VariableName == "dateValue").VariableValue);
            Assert.AreEqual("2023-03-08", job2.RequestVariables.First(rv => rv.VariableName == "startValue").VariableValue);

            repository.VerifyAll();
        }
    }
}
