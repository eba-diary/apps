using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// Used by <see cref="SecurablePermission"/> to indicate if the permission came
    /// from the <see cref="ISecurable"/> being requested, or inherited from an ancestor
    /// </summary>
    public enum SecurablePermissionScope
    {
        Self,
        Inherited
    }
}
