using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Sentry.data.Core;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class JiraServiceTests
    {
        //[TestMethod]
        //public Task CreateJiraTicketAsync_AssistanceTicket_IssueKey()
        //{
        //    MockRepository mr = new MockRepository(MockBehavior.Strict);

        //    Mock<HttpMessageHandler> httpMessageHandler = repo.Create<HttpMessageHandler>();

        //    HttpResponseMessage responseMessage = new HttpResponseMessage
        //    {
        //        Content = new StringContent(""),
        //        StatusCode = HttpStatusCode.OK
        //    };

        //    string requestUrl = $@"{dataSource.BaseUri}Search/{DateTime.Today.AddDays(-2):yyyy-MM-dd}?endDate={DateTime.Today.AddDays(-1):yyyy-MM-dd}";
        //    httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
        //                                                                    ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl),
        //                                                                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage);

        //    HttpResponseMessage emptyMessage = CreateResponseMessage("[]");
        //    httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
        //                                                                    ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.ToString() == requestUrl + "&pageNumber=2"),
        //                                                                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(emptyMessage);
        //    httpMessageHandler.Protected().Setup("Dispose", ItExpr.Is<bool>(x => x));

        //    HttpClient httpClient = new HttpClient(httpMessageHandler.Object, true);

        //    mr.VerifyAll();
        //}

        //issue type not found

        //Post request fail

        //Jira user exists

        //Jira user not found

        //Jira user failed
    }
}
