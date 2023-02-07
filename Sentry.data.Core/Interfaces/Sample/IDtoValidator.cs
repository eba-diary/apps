using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDtoValidator<T> where T : IValidatableDto
    {
        List<ValidationResultDto> Validate(T dto);
    }
}
