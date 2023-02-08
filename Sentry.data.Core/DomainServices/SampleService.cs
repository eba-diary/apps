using System.Collections.Generic;
using System.Linq;
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
            dto.SampleId = _samples.Any() ? _samples.Max(x => x.SampleId) + 1 : 1;
            _samples.Add(dto);
            return dto;
        }

        public async Task<SampleDto> GetSample(int id)
        {
            SampleDto dto = _samples.FirstOrDefault(x => x.SampleId == id);
            if (dto != null)
            {
                return dto;
            }

            throw new ResourceNotFoundException();
        }

        public async Task<SampleDto> UpdateSample(SampleDto dto)
        {
            SampleDto existing = _samples.FirstOrDefault(x => x.SampleId == dto.SampleId);

            if (existing != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Name))
                {
                    existing.Name = dto.Name;
                }

                if (!string.IsNullOrWhiteSpace(dto.Description))
                {
                    existing.Description = dto.Description;
                }

                return existing;
            }

            throw new ResourceNotFoundException();
        }
    }
}
