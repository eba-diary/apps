using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class FilterCategoryOptionNormalizerTests
    {
        [DataTestMethod]
        [DataRow(FilterCategoryOptions.ENVIRONMENT_PROD, "Prod")]
        [DataRow(FilterCategoryOptions.ENVIRONMENT_NONPROD, "NonProd")]
        [DataRow("foo", "foo")]
        public void Normalize_ProdType_Environment(string input, string output)
        {
            Assert.AreEqual(FilterCategoryOptionNormalizer.Normalize(FilterCategoryNames.ENVIRONMENT, input), output);
        }

        [DataTestMethod]
        [DataRow(FilterCategoryOptions.ENVIRONMENT_PROD, "Prod")]
        [DataRow(FilterCategoryOptions.ENVIRONMENT_NONPROD, "NonProd")]
        [DataRow("foo", "foo")]
        public void Deormalize_Environment_ProdType(string input, string output)
        {
            Assert.AreEqual(FilterCategoryOptionNormalizer.Denormalize(FilterCategoryNames.ENVIRONMENT, output), input);
        }
    }
}
