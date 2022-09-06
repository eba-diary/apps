using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nest;
using System;
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
            Nest.Fields fields = NestHelper.GetSearchFields<DataInventory>();
            
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
            AggregationDictionary fields = NestHelper.GetFilterAggregations<DataInventory>();

            Assert.AreEqual(11, fields.Count());

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.ASSET));
            Assert.IsNotNull(fields[FilterCategoryNames.DataInventory.ASSET].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.ENVIRONMENT));
            Assert.IsNotNull(fields[FilterCategoryNames.DataInventory.ENVIRONMENT].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.DATABASE));
            Assert.IsNotNull(fields[FilterCategoryNames.DataInventory.DATABASE].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.DATATYPE));
            Assert.IsNotNull(fields[FilterCategoryNames.DataInventory.DATATYPE].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.SOURCETYPE));
            Assert.IsNotNull(fields[FilterCategoryNames.DataInventory.SOURCETYPE].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.SERVER));
            Assert.IsNotNull(fields[FilterCategoryNames.DataInventory.SERVER].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.COLUMN));
            Assert.IsNotNull(fields[FilterCategoryNames.DataInventory.COLUMN].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.COLLECTIONNAME));
            Assert.IsNotNull(fields[FilterCategoryNames.DataInventory.COLLECTIONNAME].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.COLLECTIONTYPE));
            Assert.IsNotNull(fields[FilterCategoryNames.DataInventory.COLLECTIONTYPE].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.NULLABLE));
            Assert.AreEqual("IsNullable", fields[FilterCategoryNames.DataInventory.NULLABLE].Terms.Field.Property.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DataInventory.SENSITIVE));
            Assert.AreEqual("IsSensitive", fields[FilterCategoryNames.DataInventory.SENSITIVE].Terms.Field.Property.Name);
        }

        [TestMethod]
        public void FilterCategoryFields_DataInventory_AssetCode_FilterSearchField()
        {
            Field field = NestHelper.GetFilterCategoryField<DataInventory>(FilterCategoryNames.DataInventory.ASSET);            
            Assert.IsNotNull(field.Expression);
        }

        [TestMethod]
        public void FilterCategoryFields_DataInventory_NotFound_FilterSearchField()
        {
            Assert.ThrowsException<InvalidOperationException>(() => NestHelper.GetFilterCategoryField<DataInventory>("foo"));
        }
    }
}
