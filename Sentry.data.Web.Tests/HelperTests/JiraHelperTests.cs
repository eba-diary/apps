using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests.HelperTests
{
    [TestClass]
    public class JiraHelperTests
    {
        [TestMethod]
        public void GetJiraTicketUrl()
        {
            //Arrange
            string ticketId = "TIS-1111";

            //Act
            string result = JiraHelper.GetJiraTicketUrl(ticketId);

            //Asset
            Assert.AreEqual($"https://jiraqual.sentry.com/browse/{ticketId}", result);
        }

        [TestMethod]
        public void GetJiraTicketUrl_Ticket_Without_Dash()
        {
            //Arrange
            string ticketId = "1111";

            //Act
            string result = JiraHelper.GetJiraTicketUrl(ticketId);

            //Asset
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetJiraTicketUrl_Ticket_Is_Null()
        {

            //Arrange
            string ticketId = null;

            //Act
            string result = JiraHelper.GetJiraTicketUrl(ticketId);

            //Asset
            Assert.AreEqual(string.Empty, result);
        }
    }
}
