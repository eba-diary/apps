using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DaleResultDto : DaleEventableDto
    {
        public long SearchTotal { get; set; }
        public List<DaleResultRowDto> DaleResults { get; set; } = new List<DaleResultRowDto>();
    }
}
