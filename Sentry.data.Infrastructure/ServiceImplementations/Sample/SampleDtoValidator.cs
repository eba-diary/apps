using Sentry.data.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Infrastructure
{
    public class SampleDtoValidator : IDtoValidator<SampleDto>
    {
        public List<ValidationResultDto> Validate(SampleDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
