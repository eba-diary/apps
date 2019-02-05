
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IObsidianService
    {
        bool DoesGroupExist(string adGroup);
        List<string> GetAdGroups(string adGroup);
    }
}
