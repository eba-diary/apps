using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class RequestVariableTests
    {
        [TestMethod]
        public void TryIncrementVariableValue_TypeNone_False()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableIncrementType = RequestVariableIncrementType.None
            };

            bool result = variable.TryIncrementVariableValue();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryIncrementVariableValue_TypeDaily_False()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableValue = DateTime.Today.ToString("yyyy-MM-dd"),
                VariableIncrementType = RequestVariableIncrementType.Daily
            };

            bool result = variable.TryIncrementVariableValue();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryIncrementVariableValue_TypeDaily_True()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableValue = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                VariableIncrementType = RequestVariableIncrementType.Daily
            };

            bool result = variable.TryIncrementVariableValue();

            Assert.IsTrue(result);
        }
    }
}
