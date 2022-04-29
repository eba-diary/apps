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

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.ASSET));
            Assert.IsNotNull(fields[FilterCategoryNames.ASSET].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.ENVIRONMENT));
            Assert.IsNotNull(fields[FilterCategoryNames.ENVIRONMENT].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DATABASE));
            Assert.IsNotNull(fields[FilterCategoryNames.DATABASE].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.DATATYPE));
            Assert.IsNotNull(fields[FilterCategoryNames.DATATYPE].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.SOURCETYPE));
            Assert.IsNotNull(fields[FilterCategoryNames.SOURCETYPE].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.SERVER));
            Assert.IsNotNull(fields[FilterCategoryNames.SERVER].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.COLUMN));
            Assert.IsNotNull(fields[FilterCategoryNames.COLUMN].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.COLLECTIONNAME));
            Assert.IsNotNull(fields[FilterCategoryNames.COLLECTIONNAME].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.COLLECTIONTYPE));
            Assert.IsNotNull(fields[FilterCategoryNames.COLLECTIONTYPE].Terms.Field.Expression);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.NULLABLE));
            Assert.AreEqual("IsNullable", fields[FilterCategoryNames.NULLABLE].Terms.Field.Property.Name);

            Assert.IsTrue(fields.Any(x => x.Key == FilterCategoryNames.SENSITIVE));
            Assert.AreEqual("IsSensitive", fields[FilterCategoryNames.SENSITIVE].Terms.Field.Property.Name);
        }

        [TestMethod]
        public void FilterCategoryFields_DataInventory_AssetCode_FilterSearchField()
        {
            Field field = NestHelper.GetFilterCategoryField<DataInventory>(FilterCategoryNames.ASSET);            
            Assert.IsNotNull(field.Expression);
        }

        [TestMethod]
        public void FilterCategoryFields_DataInventory_NotFound_FilterSearchField()
        {
            Assert.ThrowsException<InvalidOperationException>(() => NestHelper.GetFilterCategoryField<DataInventory>("foo"));
        }
    }
}
