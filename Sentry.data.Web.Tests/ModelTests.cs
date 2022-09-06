using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void FilterCategoryOptionModel_OptionId_Encode()
        {
            FilterCategoryOptionModel model = new FilterCategoryOptionModel()
            {
                OptionValue = "Value w/ special ch@r@cters",
                ParentCategoryName = "Category"
            };

            Assert.AreEqual("Category_Value+w%2f+special+ch%40r%40cters", model.OptionId);
        }
    }
}
