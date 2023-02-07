using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISampleService
    {
        Task<SampleDto> AddSample(SampleDto dto);
        Task<SampleDto> UpdateSample(SampleDto dto);
    }
}
