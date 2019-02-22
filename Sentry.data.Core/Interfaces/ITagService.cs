using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface ITagService
    {
        bool CreateAndSaveNewTag(TagDto dto);
        bool UpdateAndSaveTag(TagDto dto);
        List<string> Validate(TagDto dto);
    }
}