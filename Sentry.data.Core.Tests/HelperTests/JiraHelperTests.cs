using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Tests.HelperTests
{
    [TestClass]
    public class JiraHelperTests
    {
        [TestMethod]
        public void BoldFormat()
        {
            //Arrange
            string textData = "My Bolded Message";

            //Act
            string result = JiraHelper.Format_Bold(textData);

            //Assert
            Assert.IsTrue(result.StartsWith("*"));
            Assert.IsTrue(result.EndsWith("*"));
            Assert.AreEqual(textData.Length + 2, result.Length);
        }

        [TestMethod]
        public void PreFormat()
        {
            //Arrange
            string textData = "My formatted text";

            //Act
            string result = JiraHelper.Format_PreFormatted(textData);

            //Assert
            Assert.IsTrue(result.StartsWith("{noformat}"));
            Assert.IsTrue(result.EndsWith("{noformat}"));
            Assert.AreEqual(textData.Length + 24, result.Length);
        }

        [TestMethod]
        public void BulletListItem()
        {
            //Arrange
            string textData = "List item text";

            //Act
            string result = JiraHelper.Format_BulletListItem(textData);

            //Assert
            Assert.IsTrue(result.StartsWith("* "));
            Assert.AreEqual(textData.Length + 2, result.Length);
        }
    }
}
