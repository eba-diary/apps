using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void TryIncrementVariableValue_TypeDailyExcludeToday_False()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableValue = DateTime.Today.ToString("yyyy-MM-dd"),
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            bool result = variable.TryIncrementVariableValue();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryIncrementVariableValue_TypeDailyExcludeToday_True()
        {
            RequestVariable variable = new RequestVariable
            {
                VariableValue = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"),
                VariableIncrementType = RequestVariableIncrementType.DailyExcludeToday
            };

            bool result = variable.TryIncrementVariableValue();

            Assert.IsTrue(result);
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
