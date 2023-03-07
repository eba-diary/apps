using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System;
using Moq;
using static Sentry.data.Infrastructure.ServiceImplementations.SecBotProvider;
using System.Net;
using Sentry.data.Infrastructure.ServiceImplementations;
using System.Threading.Tasks;
using System.Threading;
using Moq.Protected;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
            Mock<HttpMessageHandler> httpMessageHandler = mr.Create<HttpMessageHandler>();

            string thing = JsonConvert.SerializeObject(new JenkinsQueueResponse());
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(thing)
            };


            httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                                                                            ItExpr.IsAny<HttpRequestMessage>(),
                                                                            ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage).Callback(() => 
                                                                            {
                                                                                string thing2 = "hi im here";
                                                                            });
            httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

            RestClient restClient = new RestClient(httpMessageHandler.Object);
            //RestResponse<JenkinsQueueResponse>
            //arrange
            //var restClient = new Mock<RestClient>();
            //restClient.Setup(r => r.ExecuteGetAsync<JenkinsQueueResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            //    .ReturnsAsync(new RestResponse<JenkinsQueueResponse>() { StatusCode = HttpStatusCode.InternalServerError, ResponseStatus = ResponseStatus.Completed, Data = new JenkinsQueueResponse() });
            var secBotProvider = new Mock<SecBotProvider>(restClient);
            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotQueueStatus("crumb", "https://secbotdev.sentry.com/queue/1"));

            //assert
            //restClient.Verify(r => r.ExecuteGetAsync<JenkinsQueueResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        /// <summary>
        /// Tests that if the SecBot job is still not successful after the Polly retry policy has been exhausted, 
        /// an exception is thrown
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotQueueStatus_Retry_NeverSucceeds_Test()
        {
            //arrange
            var restClient = new Mock<RestClient>();
            restClient.Setup(r => r.ExecuteGetAsync<JenkinsQueueResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse<JenkinsQueueResponse>() { StatusCode = HttpStatusCode.OK, ResponseStatus = ResponseStatus.Completed, Data = new JenkinsQueueResponse() });
            var secBotProvider = new Mock<SecBotProvider>(restClient.Object);
            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotQueueStatus("crumb", "https://secbotdev.sentry.com/queue/1"));

            //assert
            restClient.Verify(r => r.ExecuteGetAsync<JenkinsQueueResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(6));
        }

        /// <summary>
        /// Tests that the Polly retry policy triggers when the intial response from SecBot/Jenkins doesn't include a Job URL
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotQueueStatus_Retry_Success_Test()
        {
            //arrange
            var jobUrl = "https://secbotdev.sentry.com/job/1";
            var restClient = new Mock<RestClient>();
            restClient.SetupSequence(r => r.ExecuteGetAsync<JenkinsQueueResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse<JenkinsQueueResponse>() { StatusCode = HttpStatusCode.OK, ResponseStatus = ResponseStatus.Completed, Data = new JenkinsQueueResponse() })
                .ReturnsAsync(new RestResponse<JenkinsQueueResponse>() { StatusCode = HttpStatusCode.OK, ResponseStatus = ResponseStatus.Completed, Data = new JenkinsQueueResponse() { executable = new Executable() { url = jobUrl } } });
            var secBotProvider = new Mock<SecBotProvider>(restClient.Object);
            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));

            //act
            var actual = await secBotProvider.Object.PollForSecBotQueueStatus("crumb", "https://secbotdev.sentry.com/queue/1");

            //assert
            Assert.AreEqual(jobUrl, actual);
            restClient.Verify(r => r.ExecuteGetAsync<JenkinsQueueResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }


        /// <summary>
        /// Tests that if we get a non-200 response from SecBot, an exception is immediately thrown (no retry)
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotJobStatus_Non200Response_Test()
        {
            //arrange
            var restClient = new Mock<RestClient>();
            restClient.Setup(r => r.ExecuteGetAsync<JenkinsJobStatusResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse<JenkinsJobStatusResponse>() { StatusCode = HttpStatusCode.InternalServerError, ResponseStatus = ResponseStatus.Completed, Data = new JenkinsJobStatusResponse() });
            var secBotProvider = new Mock<SecBotProvider>(restClient.Object);
            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));
            secBotProvider.Setup(s => s.SecBotInitialPauseTimespan()).Returns(TimeSpan.FromSeconds(0));

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotJobStatus("crumb", "https://secbotdev.sentry.com/job/1"));

            //assert
            restClient.Verify(r => r.ExecuteGetAsync<JenkinsJobStatusResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        /// <summary>
        /// Tests that if SecBot indicates the job has failed, an exception is immediately thrown (no further retry)
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotJobStatus_Failure_Test()
        {
            //arrange
            var restClient = new Mock<RestClient>();
            restClient.Setup(r => r.ExecuteGetAsync<JenkinsJobStatusResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse<JenkinsJobStatusResponse>() { StatusCode = HttpStatusCode.InternalServerError, ResponseStatus = ResponseStatus.Completed, Data = new JenkinsJobStatusResponse() { result = "FAILURE" } });
            var secBotProvider = new Mock<SecBotProvider>(restClient.Object);
            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));
            secBotProvider.Setup(s => s.SecBotInitialPauseTimespan()).Returns(TimeSpan.FromSeconds(0));

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotJobStatus("crumb", "https://secbotdev.sentry.com/job/1"));

            //assert
            restClient.Verify(r => r.ExecuteGetAsync<JenkinsJobStatusResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        /// <summary>
        /// Tests that if the SecBot job is still not successful after the Polly retry policy has been exhausted, 
        /// an exception is thrown
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotJobStatus_Retry_NeverSucceeds_Test()
        {
            //arrange
            var restClient = new Mock<RestClient>();
            restClient.Setup(r => r.ExecuteGetAsync<JenkinsJobStatusResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse<JenkinsJobStatusResponse>() { StatusCode = HttpStatusCode.OK, ResponseStatus = ResponseStatus.Completed, Data = new JenkinsJobStatusResponse() });
            var secBotProvider = new Mock<SecBotProvider>(restClient.Object);
            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));
            secBotProvider.Setup(s => s.SecBotInitialPauseTimespan()).Returns(TimeSpan.FromSeconds(0));

            //act
            await Assert.ThrowsExceptionAsync<Exceptions.SecBotProviderException>(
                () => secBotProvider.Object.PollForSecBotJobStatus("crumb", "https://secbotdev.sentry.com/job/1"));

            //assert
            restClient.Verify(r => r.ExecuteGetAsync<JenkinsJobStatusResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(6));
        }

        /// <summary>
        /// Tests that the Polly retry policy triggers when the intial response from SecBot/Jenkins doesn't indicate that it's complete
        /// </summary>
        [TestMethod]
        public async Task PollForSecBotJobStatus_Retry_Success_Test()
        {
            //arrange
            var restClient = new Mock<RestClient>();
            restClient.SetupSequence(r => r.ExecuteGetAsync<JenkinsJobStatusResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse<JenkinsJobStatusResponse>() { StatusCode = HttpStatusCode.OK, ResponseStatus = ResponseStatus.Completed, Data = new JenkinsJobStatusResponse() })
                .ReturnsAsync(new RestResponse<JenkinsJobStatusResponse>() { StatusCode = HttpStatusCode.OK, ResponseStatus = ResponseStatus.Completed, Data = new JenkinsJobStatusResponse() { result = "SUCCESS" } });
            var secBotProvider = new Mock<SecBotProvider>(restClient.Object);
            secBotProvider.CallBase = true;
            secBotProvider.Setup(s => s.RetryPauseTimespan(It.IsAny<int>())).Returns(TimeSpan.FromSeconds(0));
            secBotProvider.Setup(s => s.SecBotInitialPauseTimespan()).Returns(TimeSpan.FromSeconds(0));

            //act
            await secBotProvider.Object.PollForSecBotJobStatus("crumb", "https://secbotdev.sentry.com/job/1");

            //assert
            restClient.Verify(r => r.ExecuteGetAsync<JenkinsJobStatusResponse>(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
