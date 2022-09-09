using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class CustomAttributeHelperTests
    {
        [TestMethod]
        public void TryGetFilterCategoryName_DataInventory_Asset_True()
        {
            Assert.IsTrue(CustomAttributeHelper.TryGetFilterCategoryName<DataInventory>("asset", out string result));
            Assert.AreEqual(FilterCategoryNames.DataInventory.ASSET, result);
        }
        
        [TestMethod]
        public void TryGetFilterCategoryName_DataInventory_Foo_False()
        {
            Assert.IsFalse(CustomAttributeHelper.TryGetFilterCategoryName<DataInventory>("foo", out string result));
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetPropertiesWithAttribute_DataInventory_FilterSearchField_Properties()
        {
            IEnumerable<PropertyInfo> properties = CustomAttributeHelper.GetPropertiesWithAttribute<DataInventory, FilterSearchField>();
            Assert.AreEqual(11, properties.Count());
        }

        [TestMethod]
        public void TryGetFilterSearchFieldProperty_DataInventory_Asset_True()
        {
            Assert.IsTrue(CustomAttributeHelper.TryGetFilterSearchFieldProperty<DataInventory>("asset", out PropertyInfo result));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TryGetFilterSearchFieldProperty_DataInventory_Foo_False()
        {
            Assert.IsFalse(CustomAttributeHelper.TryGetFilterSearchFieldProperty<DataInventory>("foo", out PropertyInfo result));
            Assert.IsNull(result);
        }
    }
}
