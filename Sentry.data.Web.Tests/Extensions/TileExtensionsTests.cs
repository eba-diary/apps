using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core;
using System;

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
                TotalResults = 10,
                PageNumber = 1,
                PageSize = 5
            };

            TileResultsModel model = dto.ToModel(2, 1);
        }
    }
}
