using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface ITileSearchService<T> where T : DatasetTileDto
    {
        TileSearchResultDto<T> SearchTiles(TileSearchDto<T> searchDto);
        IEnumerable<T> SearchTileDtos(TileSearchDto<T> searchDto);
    }
}
