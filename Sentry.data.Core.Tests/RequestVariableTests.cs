using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class RequestVariableTests
    {
        [TestMethod]
        public void IncrementVariableValue_TypeNone_False()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableIncrementType = RequestVariableIncrementType.None,
                VariableValue = "1"
            };

            variable.IncrementVariableValue();

            Assert.AreEqual("1", variable.VariableValue);
        }

        [TestMethod]
        public void IncrementVariableValue_TypeDailyExcludeToday()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableValue = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"),
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            variable.IncrementVariableValue();

            Assert.AreEqual(DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), variable.VariableValue);
        }

        [TestMethod]
        public void IsValidVariableValue_TypeDailyExcludeToday_True()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableValue = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            bool result = variable.IsValidVariableValue();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidVariableValue_TypeDailyExcludeToday_False()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableValue = DateTime.Today.ToString("yyyy-MM-dd"),
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            bool result = variable.IsValidVariableValue();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValidVariableValue_TypeNone_False()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableIncrementType = RequestVariableIncrementType.None
            };

            bool result = variable.IsValidVariableValue();

            Assert.IsFalse(result);
        }
    }
}
