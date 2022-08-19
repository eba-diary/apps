using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
            List<SelectListItem> items = Utility.BuildTilePageSizeOptions("15");

            Assert.AreEqual(4, items.Count);
            Assert.IsTrue(items.FirstOrDefault(x => x.Selected).Value == "15");
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

            Assert.AreEqual(4, items.Count);

            SelectListItem item = items.First();
            Assert.AreEqual("Alphabetical", item.Text);
            Assert.AreEqual("0", item.Value);
            Assert.IsFalse(item.Selected);

            item = items[1];
            Assert.AreEqual("Favorites", item.Text);
            Assert.AreEqual("1", item.Value);
            Assert.IsFalse(item.Selected);

            item = items[2];
            Assert.AreEqual("Recently Added", item.Text);
            Assert.AreEqual("2", item.Value);
            Assert.IsTrue(item.Selected);

            item = items.Last();
            Assert.AreEqual("Recently Updated", item.Text);
            Assert.AreEqual("3", item.Value);
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
            List<PageItemModel> items = Utility.BuildPageItemList(150, 15, 5);

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
            Assert.AreEqual("10", item.PageNumber);
            Assert.IsFalse(item.IsActive);
            Assert.IsFalse(item.IsDisabled);
        }

        [TestMethod]
        public void BuildPageItemList_MultiplePagesAboveMax_PrefixEllipsis_PageItemModels()
        {

        }

        [TestMethod]
        public void BuildPageItemList_MultiplePagesAboveMax_PrefixAndSuffixEllipsis_PageItemModels()
        {

        }
    }
}
