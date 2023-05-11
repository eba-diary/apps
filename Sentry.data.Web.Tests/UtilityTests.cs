using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Extensions;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void BuildTilePageSizeOptions_15_SelectListItems()
        {
            List<SelectListItem> items = Utility.BuildTilePageSizeOptions("12");

            Assert.AreEqual(4, items.Count);
            Assert.IsTrue(items.FirstOrDefault(x => x.Selected).Value == "12");
        }

        [TestMethod]
        public void BuildDatasetSortByOptions_SelectListItems()
        {
            List<SelectListItem> items = Utility.BuildDatasetSortByOptions();

            Assert.AreEqual(5, items.Count);

            SelectListItem item = items.First();
            Assert.AreEqual("Alphabetical", item.Text);
            Assert.AreEqual("0", item.Value);
            Assert.IsTrue(item.Selected);

            item = items[1];
            Assert.AreEqual("Favorites", item.Text);
            Assert.AreEqual("1", item.Value);
            Assert.IsFalse(item.Selected);

            item = items[2];
            Assert.AreEqual("Most Accessed", item.Text);
            Assert.AreEqual("2", item.Value);
            Assert.IsFalse(item.Selected);

            item = items[3];
            Assert.AreEqual("Recently Added", item.Text);
            Assert.AreEqual("3", item.Value);
            Assert.IsFalse(item.Selected);

            item = items.Last();
            Assert.AreEqual("Recently Updated", item.Text);
            Assert.AreEqual("4", item.Value);
            Assert.IsFalse(item.Selected);
        }

        [TestMethod]
        public void BuildSelectListFromEnum_LayoutOption_SelectListItems()
        {
            List<SelectListItem> items = Utility.BuildSelectListFromEnum<LayoutOption>(1);

            Assert.AreEqual(2, items.Count);

            SelectListItem item = items.First();
            Assert.AreEqual("Grid", item.Text);
            Assert.AreEqual("0", item.Value);
            Assert.IsFalse(item.Selected);

            item = items.Last();
            Assert.AreEqual("List", item.Text);
            Assert.AreEqual("1", item.Value);
            Assert.IsTrue(item.Selected);
        }

        [TestMethod]
        public void BuildSelectListFromEnum_TileSearchSortByOption_SelectListItems()
        {
            List<SelectListItem> items = Utility.BuildSelectListFromEnum<TileSearchSortByOption>(2);

            Assert.AreEqual(3, items.Count);

            SelectListItem item = items.First();
            Assert.AreEqual("Alphabetical", item.Text);
            Assert.AreEqual("0", item.Value);
            Assert.IsFalse(item.Selected);

            item = items[1];
            Assert.AreEqual("Favorites", item.Text);
            Assert.AreEqual("1", item.Value);
            Assert.IsFalse(item.Selected);

            item = items.Last();
            Assert.AreEqual("Recent Activity", item.Text);
            Assert.AreEqual("2", item.Value);
            Assert.IsTrue(item.Selected);
        }

        [TestMethod]
        public void BuildSelectListFromEnum_GlobalDatasetSortByOption_SelectListItems()
        {
            List<SelectListItem> items = Utility.BuildSelectListFromEnum<GlobalDatasetSortByOption>((int)GlobalDatasetSortByOption.Favorites);

            Assert.AreEqual(3, items.Count);

            SelectListItem item = items.First();
            Assert.AreEqual("Relevance", item.Text);
            Assert.AreEqual("0", item.Value);
            Assert.IsFalse(item.Selected);

            item = items[1];
            Assert.AreEqual("Favorites", item.Text);
            Assert.AreEqual("1", item.Value);
            Assert.IsTrue(item.Selected);

            item = items.Last();
            Assert.AreEqual("Alphabetical", item.Text);
            Assert.AreEqual("2", item.Value);
            Assert.IsFalse(item.Selected);
        }

        [TestMethod]
        public void BuildPageItemList_NoResults_PageItemModels()
        {
            List<PageItemModel> items = Utility.BuildPageItemList(0, 15, 1);

            Assert.AreEqual(0, items.Count);
        }

        [TestMethod]
        public void BuildPageItemList_OnePage_PageItemModels()
        {
            List<PageItemModel> items = Utility.BuildPageItemList(10, 15, 1);

            Assert.AreEqual(1, items.Count);

            PageItemModel item = items.First();
            Assert.AreEqual("1", item.PageNumber);
            Assert.IsTrue(item.IsActive);
            Assert.IsFalse(item.IsDisabled);
        }

        [TestMethod]
        public void BuildPageItemList_MultiplePages_NoEllipsis_PageItemModels()
        {
            List<PageItemModel> items = Utility.BuildPageItemList(50, 15, 2);

            Assert.AreEqual(4, items.Count);

            PageItemModel item = items.First();
            Assert.AreEqual("1", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[1];
            Assert.AreEqual("2", item.PageNumber);
            Assert.IsTrue(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[2];
            Assert.AreEqual("3", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items.Last();
            Assert.AreEqual("4", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);
        }

        [TestMethod]
        public void BuildPageItemList_MultiplePages_SuffixEllipsis_PageItemModels()
        {
            List<PageItemModel> items = Utility.BuildPageItemList(300, 15, 5);

            Assert.AreEqual(9, items.Count);

            PageItemModel item = items.First();
            Assert.AreEqual("1", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[1];
            Assert.AreEqual("2", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[2];
            Assert.AreEqual("3", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[3];
            Assert.AreEqual("4", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[4];
            Assert.AreEqual("5", item.PageNumber);
            Assert.IsTrue(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[5];
            Assert.AreEqual("6", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[6];
            Assert.AreEqual("7", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[7];
            Assert.AreEqual(Pagination.ELLIPSIS, item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsTrue(item.IsDisabled);

            item = items.Last();
            Assert.AreEqual("20", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);
        }

        [TestMethod]
        public void BuildPageItemList_MultiplePagesAboveMax_PrefixEllipsis_PageItemModels()
        {
            List<PageItemModel> items = Utility.BuildPageItemList(300, 15, 20);

            Assert.AreEqual(9, items.Count);

            PageItemModel item = items.First();
            Assert.AreEqual("1", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[1];
            Assert.AreEqual(Pagination.ELLIPSIS, item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsTrue(item.IsDisabled);

            item = items[2];
            Assert.AreEqual("14", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[3];
            Assert.AreEqual("15", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[4];
            Assert.AreEqual("16", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[5];
            Assert.AreEqual("17", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[6];
            Assert.AreEqual("18", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[7];
            Assert.AreEqual("19", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items.Last();
            Assert.AreEqual("20", item.PageNumber);
            Assert.IsTrue(item.IsActive);
            Assert.IsFalse(item.IsDisabled);
        }

        [TestMethod]
        public void BuildPageItemList_MultiplePagesAboveMax_PrefixAndSuffixEllipsis_PageItemModels()
        {
            List<PageItemModel> items = Utility.BuildPageItemList(300, 15, 12);

            Assert.AreEqual(9, items.Count);

            PageItemModel item = items.First();
            Assert.AreEqual("1", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[1];
            Assert.AreEqual(Pagination.ELLIPSIS, item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsTrue(item.IsDisabled);

            item = items[2];
            Assert.AreEqual("10", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[3];
            Assert.AreEqual("11", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[4];
            Assert.AreEqual("12", item.PageNumber);
            Assert.IsTrue(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[5];
            Assert.AreEqual("13", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[6];
            Assert.AreEqual("14", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);

            item = items[7];
            Assert.AreEqual(Pagination.ELLIPSIS, item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsTrue(item.IsDisabled);

            item = items.Last();
            Assert.AreEqual("20", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);
        }

        [TestMethod]
        public void BuildSchemaDropDown()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            DatasetFileConfig config = MockClasses.MockDatasetFileConfig();
            config.Schema = new FileSchema() { SchemaId = 11, Name = "My Schema", ObjectStatus = ObjectStatusEnum.Active };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.DatasetFileConfigs).Returns(new List<DatasetFileConfig>() { config }.AsQueryable());

            var ListWithNoSelection = Utility.BuildSchemaDropDown(context.Object, 1000, 0);
            var ListWithSelection = Utility.BuildSchemaDropDown(context.Object, 1000, 11);

            Assert.AreEqual(2, ListWithNoSelection.Count());
            Assert.IsTrue(ListWithNoSelection.Any(x => x.Text == "Select Schema" && x.Selected));
            Assert.IsTrue(ListWithSelection.Any(x => x.Text == "My Schema" && !x.Selected));

            Assert.AreEqual(1, ListWithSelection.Count());
            
        }

        [TestMethod]
        public async Task SetNamedEnvironmentProperties_NonQuartermasterManagedAsset()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            MigrationRequestModel model = new MigrationRequestModel()
            {
                DatasetId = 1000,
                DatasetNamedEnvironment = "QUAL",
                SAIDAssetKeyCode = "ABCD"
            };

            Dataset dataset_Test = MockClasses.MockDataset(null, true);
            dataset_Test.Asset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            dataset_Test.ObjectStatus = ObjectStatusEnum.Active;
            dataset_Test.NamedEnvironment = "TEST";
            dataset_Test.NamedEnvironmentType = NamedEnvironmentType.NonProd;

            Dataset dataset_Qual = MockClasses.MockDataset(null, true);
            dataset_Qual.Asset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            dataset_Qual.DatasetId = 99;
            dataset_Qual.ObjectStatus = ObjectStatusEnum.Active;
            dataset_Qual.NamedEnvironment = "QUAL";
            dataset_Qual.NamedEnvironmentType = NamedEnvironmentType.NonProd;

            Dataset dataset_Prod = MockClasses.MockDataset(null, true);
            dataset_Prod.Asset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            dataset_Prod.DatasetId = 88;
            dataset_Prod.ObjectStatus = ObjectStatusEnum.Active;
            dataset_Prod.NamedEnvironment = "PROD";
            dataset_Prod.NamedEnvironmentType = NamedEnvironmentType.Prod;

            List<NamedEnvironmentDto> namedEnvironmentDtos = new List<NamedEnvironmentDto>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { dataset_Test, dataset_Qual, dataset_Prod }.AsQueryable());

            Mock<IQuartermasterService> qmService = mr.Create<IQuartermasterService>();
            qmService.Setup(s => s.GetNamedEnvironmentsAsync("ABCD")).Returns(Task.FromResult(namedEnvironmentDtos));

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");

            await model.SetNamedEnvironmentProperties(context.Object, new NamedEnvironmentBuilder(qmService.Object, dataFeatures.Object));

            Assert.IsFalse(model.QuartermasterManagedNamedEnvironments);

            Assert.AreEqual(2, model.DatasetNamedEnvironmentDropDown.Count(), "Named Environment Dropdown Count");
            Assert.IsTrue(model.DatasetNamedEnvironmentDropDown.Any(x => x.Text == "QUAL"));
            Assert.IsTrue(model.DatasetNamedEnvironmentDropDown.Any(x => x.Text == "PROD"));

            Assert.AreEqual(2, model.DatasetNamedEnvironmentTypeDropDown.Count(), "Named Environment Type Dropdown Count");
            Assert.AreEqual("NonProd", model.DatasetNamedEnvironmentTypeDropDown.Where(w => w.Selected).Select(s => s.Text).First(), "Named Environment Type Dropdown Count");
            Assert.AreEqual(NamedEnvironmentType.NonProd, model.DatasetNamedEnvironmentType);
        }


        [TestMethod]
        public async Task SetNamedEnvironmentProperties_NonQuartermasterManagedAsset_New_NamedEnvironment()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            MigrationRequestModel model = new MigrationRequestModel()
            {
                DatasetId = 1000,
                DatasetNamedEnvironment = "QUAL",
                SAIDAssetKeyCode = "ABCD"
            };

            Dataset dataset_Test = MockClasses.MockDataset(null, true);
            dataset_Test.Asset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            dataset_Test.ObjectStatus = ObjectStatusEnum.Active;
            dataset_Test.NamedEnvironment = "TEST";
            dataset_Test.NamedEnvironmentType = NamedEnvironmentType.NonProd;

            Dataset dataset_Prod = MockClasses.MockDataset(null, true);
            dataset_Prod.Asset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            dataset_Prod.DatasetId = 88;
            dataset_Prod.ObjectStatus = ObjectStatusEnum.Active;
            dataset_Prod.NamedEnvironment = "PROD";
            dataset_Prod.NamedEnvironmentType = NamedEnvironmentType.Prod;

            List<NamedEnvironmentDto> namedEnvironmentDtos = new List<NamedEnvironmentDto>();

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { dataset_Test, dataset_Prod }.AsQueryable());

            Mock<IQuartermasterService> qmService = mr.Create<IQuartermasterService>();
            qmService.Setup(s => s.GetNamedEnvironmentsAsync("ABCD")).Returns(Task.FromResult(namedEnvironmentDtos));

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");

            await model.SetNamedEnvironmentProperties(context.Object, new NamedEnvironmentBuilder(qmService.Object, dataFeatures.Object));

            Assert.IsFalse(model.QuartermasterManagedNamedEnvironments);

            Assert.AreEqual(2, model.DatasetNamedEnvironmentDropDown.Count(), "Named Environment Dropdown Count");
            Assert.IsTrue(model.DatasetNamedEnvironmentDropDown.Any(x => x.Text == "QUAL"));
            Assert.IsTrue(model.DatasetNamedEnvironmentDropDown.Any(x => x.Text == "PROD"));
            Assert.IsTrue(model.NewNonQManagedNamedEnvironment);

            Assert.AreEqual(3, model.DatasetNamedEnvironmentTypeDropDown.Count(), "Named Environment Type Dropdown Count");
            Assert.AreEqual(1, model.DatasetNamedEnvironmentTypeDropDown.Count(w => w.Selected));
            Assert.AreEqual("Select Environment Type", model.DatasetNamedEnvironmentTypeDropDown.Where(w => w.Selected).Select(s => s.Text).First(), "Named Environment Type Dropdown Count");
            Assert.AreEqual(NamedEnvironmentType.NonProd, model.DatasetNamedEnvironmentType);
            Assert.AreEqual("Select Environment Type", model.DatasetNamedEnvironmentTypeDropDown.First().Text);
        }

        [TestMethod]
        public async Task SetNamedEnvironmentProperties_QuartermasterManagedAsset()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            MigrationRequestModel model = new MigrationRequestModel()
            {
                DatasetId = 1000,
                DatasetNamedEnvironment = "QUAL",
                SAIDAssetKeyCode = "ABCD"
            };

            Dataset dataset_Test = MockClasses.MockDataset(null, true);
            dataset_Test.Asset = new Asset() { AssetId = 1, SaidKeyCode = "ABCD" };
            dataset_Test.ObjectStatus = ObjectStatusEnum.Active;
            dataset_Test.NamedEnvironment = "TEST";
            dataset_Test.NamedEnvironmentType = NamedEnvironmentType.NonProd;

            List<NamedEnvironmentDto> namedEnvironmentDtos = new List<NamedEnvironmentDto>(){
                new NamedEnvironmentDto(){ NamedEnvironment = "TEST", NamedEnvironmentType=NamedEnvironmentType.NonProd } ,
                new NamedEnvironmentDto(){ NamedEnvironment = "QUAL", NamedEnvironmentType=NamedEnvironmentType.NonProd } ,
                new NamedEnvironmentDto(){ NamedEnvironment = "PROD", NamedEnvironmentType = NamedEnvironmentType.Prod }
            };

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(s => s.Datasets).Returns(new List<Dataset>() { dataset_Test }.AsQueryable());

            Mock<IQuartermasterService> qmService = mr.Create<IQuartermasterService>();
            qmService.Setup(s => s.GetNamedEnvironmentsAsync("ABCD")).Returns(Task.FromResult(namedEnvironmentDtos));

            Mock<IDataFeatures> dataFeatures = mr.Create<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");

            await model.SetNamedEnvironmentProperties(context.Object, new NamedEnvironmentBuilder(qmService.Object, dataFeatures.Object));

            Assert.IsTrue(model.QuartermasterManagedNamedEnvironments);

            Assert.AreEqual(2, model.DatasetNamedEnvironmentDropDown.Count(), "Named Environment Dropdown Count");
            Assert.IsTrue(model.DatasetNamedEnvironmentDropDown.Any(x => x.Text == "QUAL"), "QUAL in Named Environment Dropdown");
            Assert.IsTrue(model.DatasetNamedEnvironmentDropDown.Any(x => x.Text == "PROD"), "QUAL in Named Environment Dropdown");

            Assert.AreEqual(2, model.DatasetNamedEnvironmentTypeDropDown.Count(), "Named Environment Type Dropdown Count");
            Assert.AreEqual("NonProd", model.DatasetNamedEnvironmentTypeDropDown.Where(w => w.Selected).Select(s => s.Text).First(), "Named Environment Type Dropdown Count");
            Assert.AreEqual(NamedEnvironmentType.NonProd, model.DatasetNamedEnvironmentType);
        }
    }
}
