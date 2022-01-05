using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DaleResultDto : DaleEventableDto
    {
        public List<DaleResultRowDto> DaleResults { get; set; }
    }
}
