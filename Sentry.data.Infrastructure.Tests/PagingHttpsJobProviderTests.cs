﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class PagingHttpsJobProviderTests : BaseInfrastructureUnitTest
    {
        [TestMethod]
        public void Execute_PagingTypeNone_AnonAuth_SingleLoop()
        {
            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new AnonymousAuthentication()
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.None
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 2, 1);

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

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
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{job.DataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(job.DataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, null, generator.Object, fileProvider.Object);

            provider.Execute(job);

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_TokenAuth_SingleLoop()
        {
            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new TokenAuthentication(),
                AuthenticationHeaderName = "AuthHeader"
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 2, 1);

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

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
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
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
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetTokenAuthenticationToken(dataSource)).Returns("token");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("AuthHeader", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("token", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetTokenAuthenticationToken(dataSource), Times.Exactly(1));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_OAuth_MultiplePages()
        {
            DataSourceToken token = new DataSourceToken();

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                Tokens = new List<DataSourceToken> { token }
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 2, 1);

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            int length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);

            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length++);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage responseMessage2 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            HttpResponseMessage emptyMessage = CreateResponseMessage("[]");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token)).Returns("token");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token), Times.Exactly(3));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_OAuth_MultiplePages_FailInProgress()
        {
            DataSourceToken token = new DataSourceToken { Id = 3 };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                Tokens = new List<DataSourceToken> { token }
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 2, 1);

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            long length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);
            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length = (long)Math.Pow(1024, 3) * 2);
            stream.Setup(x => x.SetLength(0)).Callback(() => length = 0);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage responseMessage2 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.SetupSequence(x => x.GetOAuthAccessToken(dataSource, token)).Returns("token").Returns("token").Throws<Exception>();
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            Assert.ThrowsException<AggregateException>(() => provider.Execute(job));

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsTrue(job.ExecutionParameters.Any());
            Assert.AreEqual(2, job.ExecutionParameters.Count());

            KeyValuePair<string, string> parameter = job.ExecutionParameters.First();
            Assert.AreEqual("pageNumber", parameter.Key);
            Assert.AreEqual("2", parameter.Value);

            parameter = job.ExecutionParameters.Last();
            Assert.AreEqual(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, parameter.Key);
            Assert.AreEqual("3", parameter.Value);

            Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token), Times.Exactly(3));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_OAuth_StartFromSavedProgress()
        {
            DataSourceToken token = new DataSourceToken { Id = 3 };
            DataSourceToken token2 = new DataSourceToken { Id = 4 };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                Tokens = new List<DataSourceToken> { token, token2 }
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 2, 1);
            job.ExecutionParameters = new Dictionary<string, string>
            {
                { options.PageParameterName, "2" },
                { ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, "4" }
            };

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            long length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);
            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length++);
            
            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage emptyMessage = CreateResponseMessage("[]");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.SetupSequence(x => x.GetOAuthAccessToken(dataSource, token2)).Returns("token2").Returns("token2");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token2", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token2), Times.Exactly(2));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_OAuth_StartFromSavedProgress_NoRequestVariables()
        {
            DataSourceToken token = new DataSourceToken { Id = 3 };
            DataSourceToken token2 = new DataSourceToken { Id = 4 };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                Tokens = new List<DataSourceToken> { token, token2 }
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 0, 0);
            job.RelativeUri = "Search";
            job.RequestVariables = new List<RequestVariable>();
            job.ExecutionParameters = new Dictionary<string, string>
            {
                { options.PageParameterName, "2" },
                { ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, "4" }
            };

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            long length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);
            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length++);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "?pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage emptyMessage = CreateResponseMessage("[]");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "?pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.SetupSequence(x => x.GetOAuthAccessToken(dataSource, token2)).Returns("token2").Returns("token2");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token2", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.IsFalse(job.RequestVariables.Any());

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token2), Times.Exactly(2));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_OAuth_MultiplePages_MultipleTokens()
        {
            DataSourceToken token = new DataSourceToken { Id = 3 };
            DataSourceToken token2 = new DataSourceToken { Id = 4 };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                Tokens = new List<DataSourceToken> { token, token2 }
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 2, 1);

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            int saveCount = 0;
            datasetContext.Setup(x => x.SaveChanges(true)).Callback(() =>
            {
                saveCount++;
                if (saveCount == 1)
                {
                    Assert.IsTrue(job.ExecutionParameters.Any());
                    Assert.AreEqual(2, job.ExecutionParameters.Count());

                    KeyValuePair<string, string> parameter = job.ExecutionParameters.First();
                    Assert.AreEqual("pageNumber", parameter.Key);
                    Assert.AreEqual("3", parameter.Value);

                    parameter = job.ExecutionParameters.Last();
                    Assert.AreEqual(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, parameter.Key);
                    Assert.AreEqual("3", parameter.Value);
                }
                else if (saveCount == 2)
                {
                    Assert.IsTrue(job.ExecutionParameters.Any());
                    Assert.AreEqual(2, job.ExecutionParameters.Count());

                    KeyValuePair<string, string> parameter = job.ExecutionParameters.First();
                    Assert.AreEqual("pageNumber", parameter.Key);
                    Assert.AreEqual("3", parameter.Value);

                    parameter = job.ExecutionParameters.Last();
                    Assert.AreEqual(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, parameter.Key);
                    Assert.AreEqual("4", parameter.Value);
                }
            });

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            long length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);
            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length += (long)Math.Pow(1024, 3));
            stream.Setup(x => x.WriteAsync(It.Is<byte[]>(b => b.SequenceEqual(Encoding.UTF8.GetBytes("\r\n"))), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            stream.Setup(x => x.SetLength(0)).Callback(() => length = 0);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage responseMessage2 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=1"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            HttpResponseMessage responseMessage3 = GetResponseMessage("PagingHttps_BasicResponse.json");
            HttpResponseMessage responseMessage4 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage3).ReturnsAsync(responseMessage4);

            HttpResponseMessage emptyMessage = CreateResponseMessage("[]");
            HttpResponseMessage emptyMessage2 = CreateResponseMessage("[]");
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage).ReturnsAsync(emptyMessage2);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token)).Returns("token");
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token2)).Returns("token2");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token2", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token), Times.Exactly(3));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token2), Times.Exactly(3));
            s3Provider.Verify(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_"))), Times.Exactly(2));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(3));
            stream.Verify(x => x.SetLength(0), Times.Exactly(2));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_TokenAuth_MultiplePages_MultipleVariableIncrements()
        {
            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new TokenAuthentication(),
                AuthenticationHeaderName = "AuthHeader"
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 3, 2);
            job.FileSchema = new FileSchema { SchemaRootPath = "Items" };

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            long length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);

            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length++);
            stream.Setup(x => x.WriteAsync(It.Is<byte[]>(b => b.SequenceEqual(Encoding.UTF8.GetBytes("\r\n"))), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_NestedResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-3):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-2):yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage responseMessage2 = GetResponseMessage("PagingHttps_NestedResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            HttpResponseMessage emptyMessage = GetResponseMessage("PagingHttps_EmptyNestedResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);



            HttpResponseMessage responseMessage3 = GetResponseMessage("PagingHttps_NestedResponse.json");
            string requestUrl2 = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl2),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage3);

            HttpResponseMessage responseMessage4 = GetResponseMessage("PagingHttps_NestedResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl2 + "&pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage4);

            HttpResponseMessage emptyMessage2 = GetResponseMessage("PagingHttps_EmptyNestedResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl2 + "&pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage2);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetTokenAuthenticationToken(dataSource)).Returns("token");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("AuthHeader", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("token", httpClient.DefaultRequestHeaders.First().Value.First());
            Assert.AreEqual(4, length);
            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetTokenAuthenticationToken(dataSource), Times.Exactly(1));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_OAuth_MultiplePages_MultipleTokens_MultipleVariableIncrements()
        {
            DataSourceToken token = new DataSourceToken { Id = 3 };
            DataSourceToken token2 = new DataSourceToken { Id = 4 };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                Tokens = new List<DataSourceToken> { token, token2 }
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 3, 2);

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            long length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);
            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length += (long)((Math.Pow(1024, 3) * 2)/3));
            stream.Setup(x => x.WriteAsync(It.Is<byte[]>(b => b.SequenceEqual(Encoding.UTF8.GetBytes("\r\n"))), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            stream.Setup(x => x.SetLength(0)).Callback(() => length = 0);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-3):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-2):yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage responseMessage2 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=1"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            HttpResponseMessage responseMessage3 = GetResponseMessage("PagingHttps_BasicResponse.json");
            HttpResponseMessage responseMessage4 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage3).ReturnsAsync(responseMessage4);

            HttpResponseMessage emptyMessage = CreateResponseMessage("[]");
            HttpResponseMessage emptyMessage2 = CreateResponseMessage("[]");
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage).ReturnsAsync(emptyMessage2);

            HttpResponseMessage responseMessage5 = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl2 = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl2),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage5);

            HttpResponseMessage responseMessage6 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl2 + "&pageNumber=1"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage6);

            HttpResponseMessage responseMessage7 = GetResponseMessage("PagingHttps_BasicResponse.json");
            HttpResponseMessage responseMessage8 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl2 + "&pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage7).ReturnsAsync(responseMessage8);

            HttpResponseMessage emptyMessage3 = CreateResponseMessage("[]");
            HttpResponseMessage emptyMessage4 = CreateResponseMessage("[]");
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl2 + "&pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage3).ReturnsAsync(emptyMessage4);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token)).Returns("token");
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token2)).Returns("token2");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token2", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token), Times.Exactly(6));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token2), Times.Exactly(6));
            s3Provider.Verify(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_"))), Times.Exactly(3));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(3));
            stream.Verify(x => x.SetLength(0), Times.Exactly(2));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypeNone_AnonAuth_FutureRequestVariable()
        {
            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new AnonymousAuthentication()
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.None
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 1, 0);

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(null, null, null, null, null);

            provider.Execute(job);
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_OAuth_MultiplePages_NoRequestVariables()
        {
            DataSourceToken token = new DataSourceToken();

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                Tokens = new List<DataSourceToken> { token }
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 2, 1);
            job.RelativeUri = "Search";
            job.RequestVariables = new List<RequestVariable>();

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            datasetContext.Setup(x => x.SaveChanges(true));

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            int length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);

            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length++);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage responseMessage2 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "?pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            HttpResponseMessage emptyMessage = CreateResponseMessage("[]");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "?pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token)).Returns("token");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.IsFalse(job.RequestVariables.Any());

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token), Times.Exactly(3));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(1));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypePageNumber_OAuth_MultiplePages_MultipleTokens_NoRequestVariables()
        {
            DataSourceToken token = new DataSourceToken { Id = 3 };
            DataSourceToken token2 = new DataSourceToken { Id = 4 };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                Tokens = new List<DataSourceToken> { token, token2 }
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.PageNumber,
                PageParameterName = "pageNumber"
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 2, 1);
            job.RelativeUri = "Search";
            job.RequestVariables = new List<RequestVariable>();

            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            int saveCount = 0;
            datasetContext.Setup(x => x.SaveChanges(true)).Callback(() =>
            {
                saveCount++;
                if (saveCount == 1)
                {
                    Assert.IsTrue(job.ExecutionParameters.Any());
                    Assert.AreEqual(2, job.ExecutionParameters.Count());

                    KeyValuePair<string, string> parameter = job.ExecutionParameters.First();
                    Assert.AreEqual("pageNumber", parameter.Key);
                    Assert.AreEqual("3", parameter.Value);

                    parameter = job.ExecutionParameters.Last();
                    Assert.AreEqual(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, parameter.Key);
                    Assert.AreEqual("3", parameter.Value);
                }
                else if (saveCount == 2)
                {
                    Assert.IsTrue(job.ExecutionParameters.Any());
                    Assert.AreEqual(2, job.ExecutionParameters.Count());

                    KeyValuePair<string, string> parameter = job.ExecutionParameters.First();
                    Assert.AreEqual("pageNumber", parameter.Key);
                    Assert.AreEqual("3", parameter.Value);

                    parameter = job.ExecutionParameters.Last();
                    Assert.AreEqual(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, parameter.Key);
                    Assert.AreEqual("4", parameter.Value);
                }
            });

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            long length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);
            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length += (long)Math.Pow(1024, 3));
            stream.Setup(x => x.WriteAsync(It.Is<byte[]>(b => b.SequenceEqual(Encoding.UTF8.GetBytes("\r\n"))), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            stream.Setup(x => x.SetLength(0)).Callback(() => length = 0);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search";
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

            HttpResponseMessage responseMessage2 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "?pageNumber=1"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage2);

            HttpResponseMessage responseMessage3 = GetResponseMessage("PagingHttps_BasicResponse.json");
            HttpResponseMessage responseMessage4 = GetResponseMessage("PagingHttps_BasicResponse.json");
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "?pageNumber=2"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage3).ReturnsAsync(responseMessage4);

            HttpResponseMessage emptyMessage = CreateResponseMessage("[]");
            HttpResponseMessage emptyMessage2 = CreateResponseMessage("[]");
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "?pageNumber=3"),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage).ReturnsAsync(emptyMessage2);
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token)).Returns("token");
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token2)).Returns("token2");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token2", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.IsFalse(job.RequestVariables.Any());

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token), Times.Exactly(3));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token2), Times.Exactly(3));
            s3Provider.Verify(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_"))), Times.Exactly(2));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(3));
            stream.Verify(x => x.SetLength(0), Times.Exactly(2));
            repo.VerifyAll();
        }

        [TestMethod]
        public void Execute_PagingTypeNone_OAuth_MultipleTokens_MultipleVariableIncrements()
        {
            DataSourceToken token = new DataSourceToken { Id = 3 };
            DataSourceToken token2 = new DataSourceToken { Id = 4 };

            HTTPSSource dataSource = new HTTPSSource
            {
                BaseUri = new Uri("https://www.base.com"),
                SourceAuthType = new OAuthAuthentication(),
                Tokens = new List<DataSourceToken> { token, token2 }
            };

            HttpsOptions options = new HttpsOptions
            {
                PagingType = PagingType.None
            };

            RetrieverJob job = GetBaseRetrieverJob(dataSource, options, 3, 2);
            
            MockRepository repo = new MockRepository(MockBehavior.Strict);

            List<DataFlowStep> steps = GetDataFlowSteps(job.DataFlow);

            Mock<IDatasetContext> datasetContext = repo.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.DataFlowStep).Returns(steps.AsQueryable());
            int saveCount = 0;
            datasetContext.Setup(x => x.SaveChanges(true)).Callback(() =>
            {
                saveCount++;
                if (saveCount == 1)
                {
                    Assert.IsTrue(job.ExecutionParameters.Any());
                    Assert.AreEqual(1, job.ExecutionParameters.Count());

                    KeyValuePair<string, string> parameter = job.ExecutionParameters.First();
                    Assert.AreEqual(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, parameter.Key);
                    Assert.AreEqual("4", parameter.Value);

                    Assert.AreEqual(DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
                    Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);
                }
                else if (saveCount == 2)
                {
                    Assert.IsTrue(job.ExecutionParameters.Any());
                    Assert.AreEqual(1, job.ExecutionParameters.Count());

                    KeyValuePair<string, string> parameter = job.ExecutionParameters.First();
                    Assert.AreEqual(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, parameter.Key);
                    Assert.AreEqual("3", parameter.Value);

                    Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
                    Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);
                }
                else if (saveCount == 3)
                {
                    Assert.IsTrue(job.ExecutionParameters.Any());
                    Assert.AreEqual(1, job.ExecutionParameters.Count());

                    KeyValuePair<string, string> parameter = job.ExecutionParameters.First();
                    Assert.AreEqual(ExecutionParameterKeys.PagingHttps.CURRENTDATASOURCETOKENID, parameter.Key);
                    Assert.AreEqual("4", parameter.Value);

                    Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
                    Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);
                }
            });

            Mock<IFileProvider> fileProvider = repo.Create<IFileProvider>();
            string expectedPath = @"C:\tmp\GoldenEye\work\Jobs\1";
            fileProvider.Setup(x => x.CreateDirectory(expectedPath));

            long length = 0;
            Mock<Stream> stream = repo.Create<Stream>();
            stream.Setup(x => x.Close());
            stream.Setup(x => x.Length).Returns(() => length);
            stream.SetupGet(x => x.CanRead).Returns(true);
            stream.SetupGet(x => x.CanWrite).Returns(true);
            stream.Setup(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Callback(() => length += (long)Math.Pow(1024, 3) * 2);
            stream.Setup(x => x.WriteAsync(It.Is<byte[]>(b => b.SequenceEqual(Encoding.UTF8.GetBytes("\r\n"))), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            stream.Setup(x => x.SetLength(0)).Callback(() => length = 0);

            string filename = expectedPath + @"\filename.json";
            fileProvider.Setup(x => x.GetFileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite)).Returns(stream.Object);
            fileProvider.Setup(x => x.DeleteFile(filename));

            Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

            HttpResponseMessage responseMessage = GetResponseMessage("PagingHttps_BasicResponse.json");
            HttpResponseMessage responseMessage2 = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-3):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-2):yyyy-MM-dd}";
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage).ReturnsAsync(responseMessage2);

            HttpResponseMessage responseMessage3 = GetResponseMessage("PagingHttps_BasicResponse.json");
            HttpResponseMessage responseMessage4 = GetResponseMessage("PagingHttps_BasicResponse.json");
            string requestUrl2 = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl2),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage3).ReturnsAsync(responseMessage4);

            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

            Mock<IHttpClientGenerator> generator = repo.Create<IHttpClientGenerator>();
            generator.Setup(x => x.GenerateHttpClient(dataSource.BaseUri.ToString())).Returns(httpClient);

            Mock<IS3ServiceProvider> s3Provider = repo.Create<IS3ServiceProvider>();
            s3Provider.Setup(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_")))).Returns("");

            Mock<IAuthorizationProvider> authorizationProvider = repo.Create<IAuthorizationProvider>();
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token)).Returns("token");
            authorizationProvider.Setup(x => x.GetOAuthAccessToken(dataSource, token2)).Returns("token2");
            authorizationProvider.Setup(x => x.Dispose());

            PagingHttpsJobProvider provider = new PagingHttpsJobProvider(datasetContext.Object, s3Provider.Object, authorizationProvider.Object, generator.Object, fileProvider.Object);

            provider.Execute(job);

            Assert.AreEqual(1, httpClient.DefaultRequestHeaders.Count());
            Assert.AreEqual("Authorization", httpClient.DefaultRequestHeaders.First().Key);
            Assert.AreEqual("Bearer token2", httpClient.DefaultRequestHeaders.First().Value.First());

            Assert.IsFalse(job.ExecutionParameters.Any());
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), job.RequestVariables.First().VariableValue);
            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), job.RequestVariables.Last().VariableValue);

            fileProvider.Verify(x => x.DeleteFile(filename), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token), Times.Exactly(2));
            authorizationProvider.Verify(x => x.GetOAuthAccessToken(dataSource, token2), Times.Exactly(2));
            s3Provider.Verify(x => x.UploadDataFile(stream.Object, "target-bucket", It.Is<string>(s => s.StartsWith("sub-folder/filename_"))), Times.Exactly(4));
            datasetContext.Verify(x => x.SaveChanges(true), Times.Exactly(4));
            stream.Verify(x => x.SetLength(0), Times.Exactly(3));
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

        private RetrieverJob GetBaseRetrieverJob(DataSource dataSource, HttpsOptions options, int dayOne, int dayTwo)
        {
            return new RetrieverJob
            {
                Id = 1,
                RelativeUri = "Search/~[var1]~?endDate=~[var2]~",
                DataSource = dataSource,
                JobOptions = new RetrieverJobOptions
                {
                    TargetFileName = "filename",
                    HttpOptions = options
                },
                DataFlow = new DataFlow { Id = 2 },
                FileSchema = new FileSchema(),
                RequestVariables = new List<RequestVariable>
                {
                    new RequestVariable
                    {
                        VariableName = "var1",
                        VariableValue = DateTime.Today.AddDays(-dayOne).ToString("yyyy-MM-dd"),
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    },
                    new RequestVariable
                    {
                        VariableName = "var2",
                        VariableValue = DateTime.Today.AddDays(-dayTwo).ToString("yyyy-MM-dd"),
                        VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
                    }
                }
            };
        }
        #endregion
    }
}