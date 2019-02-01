using System;

namespace Sentry.data.Core
{
    public interface IUserService
    {
        IApplicationUser GetByAssociateId(string associateId);
        IApplicationUser GetById(int id);
        IApplicationUser GetByDomainUser(DomainUser domainUser);
        IApplicationUser GetCurrentUser();
        IApplicationUser GetCurrentRealUser();


    }
}