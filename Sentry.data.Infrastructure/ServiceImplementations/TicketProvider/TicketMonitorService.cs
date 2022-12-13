using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.Common;
using StructureMap;

namespace Sentry.data.Infrastructure
{
    public class TicketMonitorService : ITicketMonitorService
    {
        public async Task CheckTicketStatus()
        {
            using (IContainer Container = Bootstrapper.Container.GetNestedContainer())
            {
                IBaseTicketProvider _baseTicketProvider = Container.GetInstance<IBaseTicketProvider>();
                IDatasetContext _datasetContext = Container.GetInstance<IDatasetContext>();
                ISecurityService _SecurityService = Container.GetInstance<ISecurityService>();

                List<SecurityTicket> tickets = _datasetContext.HpsmTickets.Where(x => x.TicketStatus == GlobalConstants.HpsmTicketStatus.PENDING && x.TicketId != null && !(x.TicketId.Equals("DEFAULT_SECURITY") || x.TicketId.Equals("DEFAULT_SECURITY_INHERITANCE"))).ToList();

                foreach (SecurityTicket ticket in tickets)
                {
                    HpsmTicket st = _baseTicketProvider.RetrieveTicket(ticket.TicketId);
                    if(st != null)
                    {
                        switch (st.TicketStatus)
                        {
                            case GlobalConstants.HpsmTicketStatus.APPROVED:

                                if (st.PreApproved)
                                {
                                    st.ApprovedById = ticket.RequestedById;
                                }
                                await _SecurityService.ApproveTicket(ticket, st.ApprovedById);
                                _baseTicketProvider.CloseTicket(ticket.TicketId);
                                break;
                            case GlobalConstants.HpsmTicketStatus.DENIED: //or Denied?  find out those statuses.

                                _baseTicketProvider.CloseTicket(ticket.TicketId, true);
                                _SecurityService.CloseTicket(ticket, st.RejectedById, st.RejectedReason, st.TicketStatus);
                                break;
                            case GlobalConstants.HpsmTicketStatus.WITHDRAWN:

                                _SecurityService.CloseTicket(ticket, st.RejectedById, st.RejectedReason, st.TicketStatus); //Check if the ticket was closed without approval.
                                break;
                            default:
                                break;  //do nothing, we will check again in 15 min.
                        }
                    }                    
                }
                _datasetContext.SaveChanges();
            }
        }
    }
}
