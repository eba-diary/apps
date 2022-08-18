using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class TileExtensionsTests
    {
        [TestMethod]
        public void ToModel_TileSearchResultDto_TileResultsModel()
        {
            TileSearchResultDto<DatasetTileDto> dto = new TileSearchResultDto<DatasetTileDto>()
            {
                TotalResults = 20,
                PageNumber = 2,
                PageSize = 15,
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = "Category",
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                    },
                    new FilterCategoryDto()
                    {
                        CategoryName = "Category2",
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                    }
                }
            };

            TileResultsModel model = dto.ToModel(2, 1);

            Assert.AreEqual(20, model.TotalResults);
            Assert.AreEqual("2", model.PageItems.FirstOrDefault(x => x.IsActive).PageNumber);
            Assert.AreEqual("15", model.PageSizeOptions.FirstOrDefault(x => x.Selected).Value);
            Assert.AreEqual("2", model.SortByOptions.FirstOrDefault(x => x.Selected).Value);
            Assert.AreEqual("1", model.LayoutOptions.FirstOrDefault(x => x.Selected).Value);
            Assert.AreEqual(2, model.FilterCategories.Count);
        }

        [TestMethod]
        public void ToModels_DatasetTileDtos_TileModels()
        {
            List<DatasetTileDto> dtos = new List<DatasetTileDto>()
            {
                new DatasetTileDto()
                {
                    Id = "1",
                    Name = "TileName",
                    Description = "TileDescription",
                    Status = ObjectStatusEnum.Active,
                    IsFavorite = true,
                    Category = "Category",
                    Color = "Blue",
                    IsSecured = false,
                    LastActivityDateTime = new DateTime(2022, 8, 18, 8, 0, 0),
                    CreatedDateTime = new DateTime(2022, 8, 17, 8, 0, 0)
                },
                new DatasetTileDto()
                {
                    Id = "2",
                    Name = "TileName2",
                    Description = "TileDescription2",
                    Status = ObjectStatusEnum.Deleted,
                    IsFavorite = false,
                    Category = "Category2",
                    Color = "Green",
                    IsSecured = true,
                    LastActivityDateTime = new DateTime(2021, 8, 18, 8, 0, 0),
                    CreatedDateTime = new DateTime(2021, 8, 17, 8, 0, 0)
                }
            };

            List<TileModel> models = dtos.ToModels();

            Assert.AreEqual(2, models.Count);

            TileModel model = models.First();
            Assert.AreEqual("1", model.Id);
            Assert.AreEqual("TileName", model.Name);
            Assert.AreEqual("TileDescription", model.Description);
            Assert.AreEqual("Active", model.Status);
            Assert.AreEqual("Click here to go to the Dataset Detail Page", model.TileTitle);
            Assert.AreEqual("Click to toggle favorite", model.FavoriteTitle);
            Assert.IsTrue(model.IsFavorite);
            Assert.AreEqual("Category", model.Category);
            Assert.AreEqual("Blue", model.Color);
            Assert.IsFalse(model.IsSecured);
            Assert.AreEqual("8/18/2022", model.LastActivityShortDate);
            Assert.AreEqual("8/17/2022", model.CreatedShortDate);
            Assert.IsFalse(model.IsReport);
            Assert.IsNull(model.AbbreviatedCategories);
            Assert.IsNull(model.ReportType);
            Assert.IsNull(model.UpdateFrequency);
            Assert.IsNull(model.ContactNames);
            Assert.IsNull(model.BusinessUnits);
            Assert.IsNull(model.Functions);
            Assert.IsNull(model.Tags);

            model = models.Last();
            Assert.AreEqual("2", model.Id);
            Assert.AreEqual("TileName2", model.Name);
            Assert.AreEqual("TileDescription2", model.Description);
            Assert.AreEqual("Deleted", model.Status);
            Assert.AreEqual("Dataset is marked for deletion", model.TileTitle);
            Assert.AreEqual("Dataset is marked for deletion; favorite functionality disabled", model.FavoriteTitle);
            Assert.IsFalse(model.IsFavorite);
            Assert.AreEqual("Category2", model.Category);
            Assert.AreEqual("Green", model.Color);
            Assert.IsTrue(model.IsSecured);
            Assert.AreEqual("8/18/2021", model.LastActivityShortDate);
            Assert.AreEqual("8/17/2021", model.CreatedShortDate);
            Assert.IsFalse(model.IsReport);
            Assert.IsNull(model.AbbreviatedCategories);
            Assert.IsNull(model.ReportType);
            Assert.IsNull(model.UpdateFrequency);
            Assert.IsNull(model.ContactNames);
            Assert.IsNull(model.BusinessUnits);
            Assert.IsNull(model.Functions);
            Assert.IsNull(model.Tags);
        }
    }
}
