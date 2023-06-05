using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class GoogleSearchConsoleJobProviderTests : BaseInfrastructureUnitTest
    {
        [TestMethod]
        public void Execute_NoVariableIncrement()
        {
            DataSourceToken token = new DataSourceToken { Id = 3, Enabled = true };

            RetrieverJob job = GetBaseRetrieverJob(1, token);

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            MemoryStream localStream = new MemoryStream();

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(localStream);
            fileProvider.Setup(x => x.DeleteDirectory(expectedPath));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            string request = GetRequestBody().Replace(string.Format(Indicators.REQUESTVARIABLEINDICATOR, "dateVariable"), DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
            JObject requestObj = JObject.Parse(request);

            HttpResponseMessage responseMessage = GetResponseMessage("GoogleSearchConsole_Rows.json");
            string requestUrl = $@"{job.DataSource.BaseUri}{job.RelativeUri}";
            string requestContent = requestObj.ToString();
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            requestObj.Add("startRow", 5);
            string requestContent2 = requestObj.ToString();
            HttpResponseMessage responseMessage2 = GetResponseMessage("GoogleSearchConsole_Rows.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent2),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            requestObj["startRow"] = 10;
            string requestContent3 = requestObj.ToString();
            HttpResponseMessage emptyMessage = GetResponseMessage("GoogleSearchConsole_Empty.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent3),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(job.DataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(localStream, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("").Callback<Stream, string, string>((s, b, k) =>
            {
                s.Position = 0;
                using (StreamReader streamReader = new StreamReader(s))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JToken actual = JToken.Load(jsonReader);
                    JToken expected = JToken.Parse(GetDataString("GoogleSearchConsole_ResultFile.json"));
                    Assert.IsTrue(JToken.DeepEquals(actual, expected));
                }
            });

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken((HTTPSSource)job.DataSource, token)).Returns("token");
            authorizationProvider.Setup(x => x.Dispose());

            Mock<IDataFeatures> featureFlags = repo.Create<IDataFeatures>();

            GoogleSearchConsoleJobProvider provider = new GoogleSearchConsoleJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object, featureFlags.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);

            fileProvider.Verify(x => x.DeleteDirectory(expectedPath), Times.Exactly(1));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken((HTTPSSource)job.DataSource, token), Times.Exactly(3));
            s3Provider.Verify(x => x.UploadDataFile(localStream, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_"))), Times.Exactly(1));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_WithVariableIncrement()
        {
            DataSourceToken token = new DataSourceToken { Id = 3, Enabled = true };

            RetrieverJob job = GetBaseRetrieverJob(2, token);

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            MemoryStream localStream = new MemoryStream();

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(localStream);
            fileProvider.Setup(x => x.DeleteDirectory(expectedPath));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            string request = GetRequestBody().Replace(string.Format(Indicators.REQUESTVARIABLEINDICATOR, "dateVariable"), DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"));
            JObject requestObj = JObject.Parse(request);

            string requestUrl = $@"{job.DataSource.BaseUri}{job.RelativeUri}";
            string requestContent = requestObj.ToString();
            HttpResponseMessage responseMessage = GetResponseMessage("GoogleSearchConsole_Rows.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            requestObj.Add("startRow", 5);
            string requestContent2 = requestObj.ToString();
            HttpResponseMessage responseMessage2 = GetResponseMessage("GoogleSearchConsole_Rows.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent2),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            requestObj["startRow"] = 10;
            string requestContent3 = requestObj.ToString();
            HttpResponseMessage emptyMessage = GetResponseMessage("GoogleSearchConsole_Empty.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent3),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);

            string request2 = GetRequestBody().Replace(string.Format(Indicators.REQUESTVARIABLEINDICATOR, "dateVariable"), DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
            JObject requestObj2 = JObject.Parse(request2);

            string requestContent4 = requestObj2.ToString();
            HttpResponseMessage responseMessage3 = GetResponseMessage("GoogleSearchConsole_Rows.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent4),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage3);

            requestObj2.Add("startRow", 5);
            string requestContent5 = requestObj2.ToString();
            HttpResponseMessage responseMessage4 = GetResponseMessage("GoogleSearchConsole_Rows.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent5),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage4);

            requestObj2["startRow"] = 10;
            string requestContent6 = requestObj2.ToString();
            HttpResponseMessage emptyMessage2 = GetResponseMessage("GoogleSearchConsole_Empty.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent6),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage2);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(job.DataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(localStream, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("").Callback<Stream, string, string>((s, b, k) =>
            {
                s.Position = 0;
                using (StreamReader streamReader = new StreamReader(s))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JToken actual = JToken.Load(jsonReader);
                    JToken expected = JToken.Parse(GetDataString("GoogleSearchConsole_ResultFile2.json"));
                    Assert.IsTrue(JToken.DeepEquals(actual, expected));
                }
            });

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken((HTTPSSource)job.DataSource, token)).Returns("token");
            authorizationProvider.Setup(x => x.Dispose());

            Mock<IDataFeatures> featureFlags = repo.Create<IDataFeatures>();

            GoogleSearchConsoleJobProvider provider = new GoogleSearchConsoleJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object, featureFlags.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);

            fileProvider.Verify(x => x.DeleteDirectory(expectedPath), Times.Exactly(1));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken((HTTPSSource)job.DataSource, token), Times.Exactly(6));
            s3Provider.Verify(x => x.UploadDataFile(localStream, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_"))), Times.Exactly(1));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_EndBeforeMaxIncrement()
        {
            DataSourceToken token = new DataSourceToken { Id = 3, Enabled = true };

            RetrieverJob job = GetBaseRetrieverJob(3, token);

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            MemoryStream localStream = new MemoryStream();

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(localStream);
            fileProvider.Setup(x => x.DeleteDirectory(expectedPath));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            string request = GetRequestBody().Replace(string.Format(Indicators.REQUESTVARIABLEINDICATOR, "dateVariable"), DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd"));
            JObject requestObj = JObject.Parse(request);

            string requestUrl = $@"{job.DataSource.BaseUri}{job.RelativeUri}";
            string requestContent = requestObj.ToString();
            HttpResponseMessage responseMessage = GetResponseMessage("GoogleSearchConsole_Rows.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            requestObj.Add("startRow", 5);
            string requestContent2 = requestObj.ToString();
            HttpResponseMessage responseMessage2 = GetResponseMessage("GoogleSearchConsole_Rows.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent2),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            requestObj["startRow"] = 10;
            string requestContent3 = requestObj.ToString();
            HttpResponseMessage emptyMessage = GetResponseMessage("GoogleSearchConsole_Empty.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent3),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);

            string request2 = GetRequestBody().Replace(string.Format(Indicators.REQUESTVARIABLEINDICATOR, "dateVariable"), DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"));
            JObject requestObj2 = JObject.Parse(request2);

            string requestContent4 = requestObj2.ToString();
            HttpResponseMessage responseMessage3 = GetResponseMessage("GoogleSearchConsole_Empty.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent4),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage3);

            string request3 = GetRequestBody().Replace(string.Format(Indicators.REQUESTVARIABLEINDICATOR, "dateVariable"), DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
            JObject requestObj3 = JObject.Parse(request3);

            string requestContent5 = requestObj3.ToString();
            HttpResponseMessage responseMessage4 = GetResponseMessage("GoogleSearchConsole_Empty.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl &&
                                                                                x.Content.ReadAsStringAsync().Result == requestContent5),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage4);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(job.DataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(localStream, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("").Callback<Stream, string, string>((s, b, k) =>
            {
                s.Position = 0;
                using (StreamReader streamReader = new StreamReader(s))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JToken actual = JToken.Load(jsonReader);
                    JToken expected = JToken.Parse(GetDataString("GoogleSearchConsole_ResultFile.json"));
                    Assert.IsTrue(JToken.DeepEquals(actual, expected));
                }
            });

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken((HTTPSSource)job.DataSource, token)).Returns("token");
            authorizationProvider.Setup(x => x.Dispose());

            Mock<IDataFeatures> featureFlags = repo.Create<IDataFeatures>();

            GoogleSearchConsoleJobProvider provider = new GoogleSearchConsoleJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object, featureFlags.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);

            fileProvider.Verify(x => x.DeleteDirectory(expectedPath), Times.Exactly(1));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken((HTTPSSource)job.DataSource, token), Times.Exactly(5));
            s3Provider.Verify(x => x.UploadDataFile(localStream, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_"))), Times.Exactly(1));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        #region Helpers
        private string GetRequestBody()
        {
            return @"{
  ""startDate"": ""~[dateVariable]~"",
  ""endDate"": ""~[dateVariable]~"",
  ""dimensions"": [
    ""DATE"",
    ""PAGE"",
    ""QUERY"",
    ""COUNTRY"",
    ""DEVICE""
  ]
}";
        }

        private RetrieverJob GetBaseRetrieverJob(int dayOne, DataSourceToken token)
        {
            return new RetrieverJob
            {
                Id = 1,
                RelativeUri = "sites/https%3A%2F%2Fwww.dairylandinsurance.com%2F/searchAnalytics/query",
                DataSource = new HTTPSSource
                {
                    BaseUri = new Uri("https://www.googleapis.com/webmasters/v3/"),
                    SourceAuthType = new OAuthAuthentication(),
                    AllTokens = new List<DataSourceToken> { token }
                },
                JobOptions = new RetrieverJobOptions
                {
                    TargetFileName = "filename",
                    HttpOptions = new HttpsOptions
                    {
                        RequestMethod = HttpMethods.post,
                        Body = GetRequestBody(),
                        PagingType = PagingType.Index,
                        PageParameterName = "startRow"
                    }
                },
                DataFlow = new DataFlow { Id = 2 },
                FileSchema = new FileSchema(),
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "dateVariable",
                        VariableValue = DateTime.Today.AddDays(-dayOne).ToString("yyyy-MM-dd"),
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    }
                }
            };
        }

        private List<DataFlowStep> GetDataFlowSteps(DataFlow dataFlow)
        {
            return new List<DataFlowStep>
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
        }
        #endregion
    }
}
