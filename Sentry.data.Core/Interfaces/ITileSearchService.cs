using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ITileSearchService<T> where T : DatasetTileDto
    {
        TileSearchResultDto<T> SearchTiles(TileSearchDto<T> searchDto);
        List<T> GetSearchableTiles();
        Task PublishSearchEventAsync(TileSearchEventDto eventDto);
    }
}
