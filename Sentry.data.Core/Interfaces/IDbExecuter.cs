using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Sentry.data.Core
{
    public interface IDbExecuter
    {
        void ExecuteCommand(object parameter);

        DataTable ExecuteQuery(DateTime timeCreated);
    }
}
