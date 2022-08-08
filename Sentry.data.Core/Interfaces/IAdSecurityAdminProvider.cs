using Sentry.data.Core.DTO.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// This interface is for the service that handles AD administration,
    /// including the ability to create AD groups
    /// </summary>
    public interface IAdSecurityAdminProvider
    {
        /// <summary>
        /// Makes an authenticated call to SecBot to create an AD group
        /// </summary>
        Task CreateAdSecurityGroupAsync(AdSecurityGroupDto adSecurityGroupDto);
    }
}
