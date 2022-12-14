using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class PagingHttpsJobProviderTests : BaseInfrastructureUnitTest
    {
        [TestMethod]
        public void Execute_PagingTypeNone_SingleLoop()
        {
            DataFlow dataFlow = new DataFlow { Id = 2 };

            RetrieverJob job = new RetrieverJob
            {
                Id = 1,
                RelativeUri = "Search/~[var1]~?endDate=~[var2]~",
                DataSource = new HTTPSSource
                {
                    BaseUri = new Uri("https://www.base.com"),
                    SourceAuthType = new AnonymousAuthentication()
                },
                JobOptions = new RetrieverJobOptions
                {
                    TargetFileName = "filename",
                    HttpOptions = new HttpsOptions
                    {
                        PagingType = PagingType.None
                    }
                },
                DataFlow = dataFlow,
                FileSchema = new FileSchema(),
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "var1",
                        VariableValue = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"),
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    },
                    new RequestVariable
                    {
                        VariableName = "var2",
                        VariableValue = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = new List<DataFlowStep>
            {
                new DataFlowStep
                {
                    DataFlow = dataFlow,
                    DataAction_Type_Id = DataActionType.ProducerS3Drop,
                    TriggerBucket = "target-bucket",
                    TriggerKey = "sub-folder/"
                },
                new DataFlowStep
                {
                    DataFlow = dataFlow,
                    DataAction_Type_Id = DataActionType.ConvertParquet
                }
            };

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));
            
            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.SetupSequence(x => x.Length).Returns(0).Returns(1);

            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_NoPageResponse.json");
            string requestUrl = $@"{job.DataSource.BaseUri}Search/{DateTime.Today.AddDays(-1):yyyy-MM-dd}?endDate={DateTime.Today:yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(job.DataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", "sub-folder/filename_1.json")).Returns("");

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, null, generator.Object, fileProvider.Object);

            provider.Execute(job);

            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_SingleLoop()
        {
            DataFlow dataFlow = new DataFlow { Id = 2 };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new TokenAuthentication(),
                AuthenticationHeaderName = "AuthHeader"
            };

            RetrieverJob job = new RetrieverJob
            {
                Id = 1,
                RelativeUri = "Search/~[var1]~?endDate=~[var2]~",
                DataSource = dataSource,
                JobOptions = new RetrieverJobOptions
                {
                    TargetFileName = "filename",
                    HttpOptions = new HttpsOptions
                    {
                        PagingType = PagingType.PageNumber,
                        PageParameterName = "pageNumber"
                    }
                },
                DataFlow = dataFlow,
                FileSchema = new FileSchema(),
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "var1",
                        VariableValue = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"),
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    },
                    new RequestVariable
                    {
                        VariableName = "var2",
                        VariableValue = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                        VariableIncrementType = RequestVariableIncrementType.Daily
                    }
                }
            };

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = new List<DataFlowStep>
            {
                new DataFlowStep
                {
                    DataFlow = dataFlow,
                    DataAction_Type_Id = DataActionType.ProducerS3Drop,
                    TriggerBucket = "target-bucket",
                    TriggerKey = "sub-folder/"
                },
                new DataFlowStep
                {
                    DataFlow = dataFlow,
                    DataAction_Type_Id = DataActionType.ConvertParquet
                }
            };

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.SetupSequence(x => x.Length).Returns(0).Returns(1).Returns(1).Returns(1);

            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_NoPageResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-1):yyyy-MM-dd}?endDate={DateTime.Today:yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage emptyMessage = CreateResponseMessage("[]");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", "sub-folder/filename_1.json")).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetTokenAuthenticationToken(dataSource)).Returns("token");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("AuthHeader", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("token", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
            Assert.AreEqual(DateTime.Today.ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);

            authorizationProvider.Verify(x => x.GetTokenAuthenticationToken(dataSource), Times.Exactly(1));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        #region Helpers
        private HttpResponseMessage GetResponseMessage(string filename)
        {
            return CreateResponseMessage(GetDataString(filename));
        }

        private HttpResponseMessage CreateResponseMessage(string data)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(data),
                StatusCode = HttpStatusCode.OK
            };
        }
        #endregion
    }
}
