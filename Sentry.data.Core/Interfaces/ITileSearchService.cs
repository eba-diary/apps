using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ITileSearchService<T> where T : DatasetTileDto
    {
        TileSearchResultDto<T> SearchDatasets(TileSearchDto<T> datasetSearchDto);
        IEnumerable<T> SearchDatasetTileDtos(TileSearchDto<T> datasetSearchDto);
    }
}
