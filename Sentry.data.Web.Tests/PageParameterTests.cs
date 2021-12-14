using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class PageParameterTests
    {
        [TestMethod]
        public void PageParameters_Null_PageNumber()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, null);

            // Assert
            Assert.AreEqual(1, pageParams.PageNumber);
        }

        [TestMethod]
        public void PageParameters_Non_Null_PageNumber_Value()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(123, null);

            // Assert
            Assert.AreEqual(123, pageParams.PageNumber);
        }

        [TestMethod]
        public void PageParameters_PageSize_Null_Value()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, null);

            // Assert
            Assert.AreEqual(10, pageParams.PageSize);
        }

        [TestMethod]
        public void PageParameters_PageSize_Value_Greater_Than_Max_Size_Is_Defaulted_To_MaxPageSize()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, 10001);

            // Assert
            Assert.AreEqual(10000, pageParams.PageSize);
        }

        [TestMethod]
        public void PageParameters_PageSize_Non_Null_Value_Is_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, 543);

            // Assert
            Assert.AreEqual(543, pageParams.PageSize);
        }

        [TestMethod]
        public void PageParameters_PageSize_Zero_Value_Is_Defaulted()
        {
            // Arrage
            PageParameters pageParams = new PageParameters(null, 543);

            // Assert
            Assert.AreEqual(543, pageParams.PageSize);
        }
    }
}
