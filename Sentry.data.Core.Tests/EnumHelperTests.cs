using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Helpers;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class EnumHelperTests
    {
        [TestMethod]
        public void GetByDescription_PendingDelete()
        {
            ObjectStatusEnum result = EnumHelper.GetByDescription<ObjectStatusEnum>("pending delete");

            Assert.AreEqual(ObjectStatusEnum.Pending_Delete, result);
        }

        [TestMethod]
        public void GetByDescription_Default()
        {
            ObjectStatusEnum result = EnumHelper.GetByDescription<ObjectStatusEnum>("foo");

            Assert.AreEqual((ObjectStatusEnum)0, result);
        }
    }
}
