using Sentry.data.Core;
using StructureMap;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class TicketMonitorService : ITicketMonitorService
    {
        public async Task CheckTicketStatus()
        {
            using (IContainer Container = Bootstrapper.Container.GetNestedContainer())
            {
                string ticketProviderName = "Cherwell";
                if (Container.GetInstance<IDataFeatures>().CLA4993_JSMTicketProvider.GetValue())
                {
                    ticketProviderName = "JSM";
                }
                ITicketProvider _baseTicketProvider = Container.GetInstance<ITicketProvider>(ticketProviderName);
                IDatasetContext _datasetContext = Container.GetInstance<IDatasetContext>();
                ISecurityService _SecurityService = Container.GetInstance<ISecurityService>();

                List<SecurityTicket> tickets = _datasetContext.HpsmTickets.Where(x => x.TicketStatus == GlobalConstants.ChangeTicketStatus.PENDING && x.TicketId != null && !x.TicketId.Equals("DEFAULT_SECURITY") && !x.TicketId.Equals("DEFAULT_SECURITY_INHERITANCE")).ToList();

                foreach (SecurityTicket ticket in tickets)
                {
                    ChangeTicket st = await _baseTicketProvider.RetrieveTicketAsync(ticket.TicketId);
                    if(st != null)
                    {
                        switch (st.TicketStatus)
                        {
                            case GlobalConstants.ChangeTicketStatus.APPROVED:

                                if (st.PreApproved)
                                {
                                    st.ApprovedById = ticket.RequestedById;
                                }
                                await _SecurityService.ApproveTicket(ticket, st.ApprovedById);
                                await _baseTicketProvider.CloseTicketAsync(st);
                                break;
                            case GlobalConstants.ChangeTicketStatus.DENIED: //or Denied?  find out those statuses.

                                await _baseTicketProvider.CloseTicketAsync(st);
                                _SecurityService.CloseTicket(ticket, st.RejectedById, st.RejectedReason, st.TicketStatus);
                                break;
                            case GlobalConstants.ChangeTicketStatus.WITHDRAWN:

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
