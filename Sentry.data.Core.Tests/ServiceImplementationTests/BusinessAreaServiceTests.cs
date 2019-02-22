using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class BusinessAreaServiceTests : BaseCoreUnitTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            TestInitialize();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TestCleanup();
        }

        [TestMethod]
        public void Get_Rows_For_Business_Area_Type()
        {
            IDatasetContext dsContext = MockRepository.GenerateMock<IDatasetContext>();
            dsContext.Stub(x => x.BusinessAreaTileRows).Return(BuildMockTileRow(BusinessAreaType.PersonalLines)).Repeat.Any();

            _container.Inject(dsContext);
            var baSrvc = _container.GetInstance<IBusinessAreaService>();
            List<BusinessAreaTileRowDto> rows = baSrvc.GetRows(BusinessAreaType.PersonalLines);

            Assert.IsNotNull(rows);
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual(2, rows[0].ColumnSpan);
            Assert.AreEqual(1, rows[0].Sequence);
            Assert.AreEqual(2, rows[0].Tiles.Count);
            Assert.AreEqual("blue", rows[0].Tiles[0].TileColor);
            Assert.AreEqual("Meeting2.jpg", rows[0].Tiles[1].ImageName);
        }

        private IQueryable<BusinessAreaTileRow> BuildMockTileRow(BusinessAreaType baType)
        {
            List<BusinessAreaTileRow> tileRows = new List<BusinessAreaTileRow>
            {
                new BusinessAreaTileRow
                {
                    BusinessAreaType = baType,
                    ColumnSpan = 2,
                    Sequence = 1,
                    Id = 1,
                    Tiles = new List<BusinessAreaTile>
                    {
                        new BusinessAreaTile
                        {
                            Id = 1,
                            Title = "PL Report & Data Requests",
                            TileColor = "blue",
                            ImageName = "Collaboration5.jpg",
                            Hyperlink = "http://sharepoint.sentry.com/",
                            LinkText = "View Requests",
                            Sequence = 1
                        },
                        new BusinessAreaTile
                        {
                            Id = 2,
                            Title = "Business Intelligence",
                            TileColor = "lt_blue",
                            ImageName = "Meeting2.jpg",
                            Hyperlink = "https://data.sentry.com/Search/BusinessIntelligence/Index?category=Personal%20Lines",
                            LinkText = "View Business Intelligence",
                            Sequence = 2
                        }
                    }
                }
            };

            return tileRows.AsQueryable();
        }
    }
}