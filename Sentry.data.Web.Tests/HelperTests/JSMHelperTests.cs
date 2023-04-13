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
    public class JSMHelperTests
    {
        [TestMethod]
        public void GetJsmTicketUrl()
        {
            //Arrange
            string ticketId = "TIS-1111";

            //Act
            string result = JSMHelper.GetJsmTicketUrl(ticketId);

            //Asset
            Assert.AreEqual($"https://jsmqual.sentry.com/browse/{ticketId}", result);
        }

        [TestMethod]
        public void GetJsmTicketUrl_Ticket_Without_Dash()
        {
            //Arrange
            string ticketId = "123456";

            //Act
            string result = JSMHelper.GetJsmTicketUrl(ticketId);

            //Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetJsmTicketUrl_Ticket_Is_Null()
        {
            //Arrange
            string ticketId = null;

            //Act
            string result = JSMHelper.GetJsmTicketUrl(ticketId);

            //Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}
