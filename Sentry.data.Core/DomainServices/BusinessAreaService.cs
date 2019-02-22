using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BusinessAreaService : IBusinessAreaService
    {
        private readonly IDatasetContext _datasetContext;

        public BusinessAreaService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public List<BusinessAreaTileRowDto> GetRows(BusinessAreaType businessAreaType)
        {
            // retrieve the tiles from the database for the chosen business area
            List<BusinessAreaTileRow> rows = _datasetContext.BusinessAreaTileRows.Where(x => x.BusinessAreaType == businessAreaType).ToList();

            List<BusinessAreaTileRowDto> dtoRows = new List<BusinessAreaTileRowDto>();

            // loop through the list of tiles, building Dto versions
            foreach (BusinessAreaTileRow row in rows)
            {
                dtoRows.Add(MapToRowDto(row));
            }

            return dtoRows.OrderBy(x => x.Sequence).ToList();
        }

        private BusinessAreaTileRowDto MapToRowDto(BusinessAreaTileRow row)
        {
            return new BusinessAreaTileRowDto
            {
                Id = row.Id,
                ColumnSpan = row.ColumnSpan,
                Sequence = row.Sequence,
                Tiles = MapTilesToDto(row.Tiles.ToList())
            };
        }

        private List<BusinessAreaTileDto> MapTilesToDto(List<BusinessAreaTile> tiles)
        {
            List<BusinessAreaTileDto> dtoTiles = new List<BusinessAreaTileDto>();

            // loop through each of the tiles
            foreach (BusinessAreaTile tile in tiles)
            {
                // build a new tile dto object
                dtoTiles.Add(new BusinessAreaTileDto
                {
                    Id = tile.Id,
                    Title = tile.Title,
                    TileColor = tile.TileColor,
                    ImageName = tile.ImageName,
                    LinkText = tile.LinkText,
                    Hyperlink = tile.Hyperlink,
                    Sequence = tile.Sequence
                });
            }

            return dtoTiles.OrderBy(x => x.Sequence).ToList();
        }
    }
}