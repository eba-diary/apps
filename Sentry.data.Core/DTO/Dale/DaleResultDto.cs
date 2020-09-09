using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DaleResultDto
    {
        public List<DaleResultRowDto> DaleResults { get; set; }
        public DaleEventDto DaleEvent { get; set; }
    }
}
