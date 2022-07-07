using System.Collections.Generic;
using System.Data.SqlClient;

namespace Sentry.data.Core
{
    public interface IDbExecuter<T>
    {
        void ExecuteCommand(object parameter);

        List<T> ExecuteQuery(int timeCreated);
    }
}
