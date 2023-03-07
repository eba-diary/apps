using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NHibernate.Util;
using RestSharp;
using Sentry.data.Infrastructure.ServiceImplementations;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Sentry.data.Infrastructure.ServiceImplementations.SecBotProvider;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class SecBotProviderTests
    {
        /// <summary>
        /// Tests that if we get a non-200 response from SecBot, an exception is immediately thrown (no retry)
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotQueueStatus_Non200Response_Test()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            string secBotUrl = GetSecBotQueueUrl();

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();
            HttpResponseMessage responseMessage = GetHttpResponseMessage(HttpStatusCode.InternalServerError, new JenkinsQueueResponse());
            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri.ToString() == $"{secBotUrl}api/json" &&
                    r.Headers.GetValues("Jenkins-Crumb").First() == "crumb"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            RestClient restClient = new RestClient(httpMessageHandler.Object);
            var secBotProvider = mr.Create<SecBotProvider>(restClient);
            secBotProvider.CallBase = true;

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotQueueStatus("crumb", secBotUrl));

            httpMessageHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            mr.VerifyAll();
        }

        /// <summary>
        /// Tests that if the SecBot job is still not successful after the Polly retry policy has been exhausted, 
        /// an exception is thrown
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotQueueStatus_Retry_NeverSucceeds_Test()
        {
            //arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            string secBotUrl = GetSecBotQueueUrl();

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri.ToString() == $"{secBotUrl}api/json" &&
                    r.Headers.GetValues("Jenkins-Crumb").First() == "crumb"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsQueueResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsQueueResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsQueueResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsQueueResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsQueueResponse()));

            RestClient restClient = new RestClient(httpMessageHandler.Object, false);
            var secBotProvider = mr.Create<SecBotProvider>(restClient);
            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotQueueStatus("crumb", secBotUrl));

            //assert
            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(6), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        /// <summary>
        /// Tests that the Polly retry policy triggers when the intial response from SecBot/Jenkins doesn't include a Job URL
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotQueueStatus_Retry_Success_Test()
        {
            //arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            string secBotUrl = GetSecBotQueueUrl();
            var jobUrl = GetSecBotJobUrl();

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri.ToString() == $"{secBotUrl}api/json" &&
                    r.Headers.GetValues("Jenkins-Crumb").First() == "crumb"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsQueueResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsQueueResponse { executable = new Executable() { url = jobUrl } }));

            RestClient restClient = new RestClient(httpMessageHandler.Object, false);
            var secBotProvider = mr.Create<SecBotProvider>(restClient);

            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));

            //act
            var actual = await secBotProvider.Object.PollForSecBotQueueStatus("crumb", secBotUrl);

            //assert
            Assert.AreEqual(jobUrl, actual);
            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            mr.VerifyAll();
        }


        /// <summary>
        /// Tests that if we get a non-200 response from SecBot, an exception is immediately thrown (no retry)
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotJobStatus_Non200Response_Test()
        {
            //arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            string secBotUrl = GetSecBotJobUrl();

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri.ToString() == $"{secBotUrl}api/json" &&
                    r.Headers.GetValues("Jenkins-Crumb").First() == "crumb"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.InternalServerError, new JenkinsJobStatusResponse()));

            RestClient restClient = new RestClient(httpMessageHandler.Object, false);
            var secBotProvider = mr.Create<SecBotProvider>(restClient);

            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.SecBotInitialPauseTimespan()).Returns(TimeSpan.FromSeconds(0));

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotJobStatus("crumb", secBotUrl));

            //assert
            httpMessageHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            mr.VerifyAll();
        }

        /// <summary>
        /// Tests that if SecBot indicates the job has failed, an exception is immediately thrown (no further retry)
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotJobStatus_Failure_Test()
        {
            //arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            string secBotUrl = GetSecBotJobUrl();

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri.ToString() == $"{secBotUrl}api/json" &&
                    r.Headers.GetValues("Jenkins-Crumb").First() == "crumb"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.InternalServerError, new JenkinsJobStatusResponse() { result = "FAILURE" }));

            RestClient restClient = new RestClient(httpMessageHandler.Object, false);
            var secBotProvider = mr.Create<SecBotProvider>(restClient);

            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.SecBotInitialPauseTimespan()).Returns(TimeSpan.FromSeconds(0));

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotJobStatus("crumb", secBotUrl));

            //assert
            httpMessageHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            mr.VerifyAll();
        }

        /// <summary>
        /// Tests that if the SecBot job is still not successful after the Polly retry policy has been exhausted, 
        /// an exception is thrown
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotJobStatus_Retry_NeverSucceeds_Test()
        {
            //arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            string secBotUrl = GetSecBotJobUrl();

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri.ToString() == $"{secBotUrl}api/json" &&
                    r.Headers.GetValues("Jenkins-Crumb").First() == "crumb"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsJobStatusResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsJobStatusResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsJobStatusResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsJobStatusResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsJobStatusResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsJobStatusResponse()));

            RestClient restClient = new RestClient(httpMessageHandler.Object, false);
            var secBotProvider = mr.Create<SecBotProvider>(restClient);

            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));
            secBotProvider.Setup(s => s.SecBotInitialPauseTimespan()).Returns(TimeSpan.FromSeconds(0));

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotJobStatus("crumb", secBotUrl));

            //assert
            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(6), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            mr.VerifyAll();
        }

        /// <summary>
        /// Tests that the Polly retry policy triggers when the intial response from SecBot/Jenkins doesn't indicate that it's complete
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotJobStatus_Retry_Success_Test()
        {
            //arrange
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            string secBotUrl = GetSecBotJobUrl();

            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();
            httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri.ToString() == $"{secBotUrl}api/json" &&
                    r.Headers.GetValues("Jenkins-Crumb").First() == "crumb"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsJobStatusResponse()))
                .ReturnsAsync(GetHttpResponseMessage(HttpStatusCode.OK, new JenkinsJobStatusResponse() { result = "SUCCESS" }));

            RestClient restClient = new RestClient(httpMessageHandler.Object, false);
            var secBotProvider = mr.Create<SecBotProvider>(restClient);

            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));
            secBotProvider.Setup(s => s.SecBotInitialPauseTimespan()).Returns(TimeSpan.FromSeconds(0));

            //act
            await secBotProvider.Object.PollForSecBotJobStatus("crumb", secBotUrl);

            //assert
            httpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            mr.VerifyAll();
        }

        #region Helpers
        private HttpResponseMessage GetHttpResponseMessage(HttpStatusCode statusCode, object response)
        {
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
                RequestMessage = new HttpRequestMessage()
            };
        }

        private string GetSecBotQueueUrl()
        {
            return "https://secbotdev.sentry.com/queue/1/";
        }

        private string GetSecBotJobUrl()
        {
            return "https://secbotdev.sentry.com/job/1/";
        }
        #endregion
    }
}
