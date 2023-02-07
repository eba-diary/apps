using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SampleService : ISampleService
    {
        private IList<SampleDto> _samples;

        public SampleService(IList<SampleDto> samples)
        {
            _samples = samples;
        }

        public async Task<SampleDto> AddSample(SampleDto dto)
        {
            dto.SampleId = _samples.Max(x => x.SampleId) + 1;
            _samples.Add(dto);
            return dto;
        }

        public async Task<SampleDto> UpdateSample(SampleDto dto)
        {
            SampleDto existing = _samples.FirstOrDefault(x => x.SampleId == dto.SampleId);
            existing.Name = dto.Name;
            existing.Description = dto.Description;

            return existing;
        }
    }
}
