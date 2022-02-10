using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class NestHelperTests
    {
        [TestMethod]
        public void SearchFields_DataInventory_GlobalSearchFields()
        {
            Nest.Fields fields = NestHelper.SearchFields<DataInventory>();
            
            Assert.AreEqual(7, fields.Count());

            Assert.IsNotNull(fields.FirstOrDefault(x => x.Property.Name == "AssetCode"));
            Assert.IsNotNull(fields.FirstOrDefault(x => x.Property.Name == "BaseName"));
            Assert.IsNotNull(fields.FirstOrDefault(x => x.Property.Name == "ColumnName"));
            Assert.IsNotNull(fields.FirstOrDefault(x => x.Property.Name == "DatabaseName"));
            Assert.IsNotNull(fields.FirstOrDefault(x => x.Property.Name == "ServerName"));
            Assert.IsNotNull(fields.FirstOrDefault(x => x.Property.Name == "SourceName"));
            Assert.IsNotNull(fields.FirstOrDefault(x => x.Property.Name == "TypeDescription"));
        }

        [TestMethod]
        public void FilterCategoryFields_DataInventory_FilterSearchFieldDictionary()
        {
            Dictionary<string, Field> fields = NestHelper.FilterCategoryFields<DataInventory>();

            Assert.AreEqual(11, fields.Count);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.ASSET));
            Assert.IsNotNull(fields[FilterCategoryNames.ASSET].Expression);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.ENVIRONMENT));
            Assert.IsNotNull(fields[FilterCategoryNames.ENVIRONMENT].Expression);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.DATABASE));
            Assert.IsNotNull(fields[FilterCategoryNames.DATABASE].Expression);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.DATATYPE));
            Assert.IsNotNull(fields[FilterCategoryNames.DATATYPE].Expression);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.SOURCETYPE));
            Assert.IsNotNull(fields[FilterCategoryNames.SOURCETYPE].Expression);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.SERVER));
            Assert.IsNotNull(fields[FilterCategoryNames.SERVER].Expression);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.COLUMN));
            Assert.IsNotNull(fields[FilterCategoryNames.COLUMN].Expression);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.TABLEVIEWNAME));
            Assert.IsNotNull(fields[FilterCategoryNames.TABLEVIEWNAME].Expression);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.TYPE));
            Assert.IsNotNull(fields[FilterCategoryNames.TYPE].Expression);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.NULLABLE));
            Assert.AreEqual("IsNullable", fields[FilterCategoryNames.NULLABLE].Property.Name);

            Assert.IsTrue(fields.ContainsKey(FilterCategoryNames.SENSITIVE));
            Assert.AreEqual("IsSensitive", fields[FilterCategoryNames.SENSITIVE].Property.Name);
        }

        [TestMethod]
        public void FilterCategoryFields_DataInventory_AssetCode_FilterSearchField()
        {
            Field field = NestHelper.FilterCategoryField<DataInventory>(FilterCategoryNames.ASSET);            
            Assert.AreEqual("AssetCode", field.Property.Name);
        }

        [TestMethod]
        public void FilterCategoryFields_DataInventory_NotFound_FilterSearchField()
        {
            Assert.ThrowsException<InvalidOperationException>(() => NestHelper.FilterCategoryField<DataInventory>("foo"));
        }
    }
}
