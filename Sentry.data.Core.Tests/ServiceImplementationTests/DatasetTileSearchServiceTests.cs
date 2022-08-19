using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class DatasetTileSearchServiceTests : BaseCoreUnitTest
    {
        [TestMethod]
        public void PublishSearchEventAsync_TileSearchEventDto_Success()
        {
            JObject searchJson = GetData("DatasetTileSearchService_TileSearchEventDto.json");

            Mock<IEventService> eventService = new Mock<IEventService>(MockBehavior.Strict);
            eventService.Setup(x => x.PublishSuccessEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Callback<string, string, string>((type, reason, search) =>
            {
                Assert.AreEqual(GlobalConstants.EventType.SEARCH, type);
                Assert.AreEqual("Searched Datasets", reason);
                Assert.AreEqual(searchJson.ToString(Formatting.None), search);
            }).Returns(Task.CompletedTask);

            TileSearchEventDto dto = new TileSearchEventDto()
            {
                SearchName = "SearchName",
                SearchText = "SearchText",
                FilterCategories = new List<FilterCategoryDto>()
                {
                    new FilterCategoryDto()
                    {
                        CategoryName = FilterCategoryNames.Dataset.CATEGORY,
                        DefaultCategoryOpen = true,
                        CategoryOptions = new List<FilterCategoryOptionDto>()
                        {
                            new FilterCategoryOptionDto()
                            {
                                ParentCategoryName = FilterCategoryNames.Dataset.CATEGORY,
                                OptionValue = "Sentry",
                                Selected = true
                            }
                        }
                    }
                },
                PageNumber = 2,
                PageSize = 15,
                SortBy = 3,
                Layout = 1,
                TotalResults = 50
            };

            DatasetTileSearchService service = new DatasetTileSearchService(null, null, eventService.Object);
            service.PublishSearchEventAsync(dto).Wait();
        }

        [TestMethod]
        public void SearchTiles_TileSearchDto_DatasetTileDto_TileSearchResultDto()
        {

        }
    }
}
