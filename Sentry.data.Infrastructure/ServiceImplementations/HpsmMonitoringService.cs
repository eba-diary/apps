using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core;
using StructureMap;

namespace Sentry.data.Infrastructure
{
    public class HpsmMonitoringService : IHpsmMonitoringService
    {
        private IDatasetContext _datasetContext;
        private IHpsmProvider _hpsmProvider;
        private ISecurityService _SecurityService;
        private IContainer _container;
        public HpsmMonitoringService() { }

        public void CheckHpsmTicketStatus()
        {
            using (_container = Bootstrapper.Container.GetNestedContainer())
            {
                _datasetContext = _container.GetInstance<IDatasetContext>();
                _hpsmProvider = _container.GetInstance<IHpsmProvider>();
                _SecurityService = _container.GetInstance<ISecurityService>();

                List<SecurityTicket> tickets = _datasetContext.HpsmTickets.Where(x => x.TicketStatus == GlobalConstants.HpsmTicketStatus.PENDING).ToList();

                foreach (SecurityTicket ticket in tickets)
                {
                    HpsmTicket st = _hpsmProvider.RetrieveTicket(ticket.TicketId);
                    switch (st.TicketStatus)
                    {
                        case GlobalConstants.HpsmTicketStatus.APPROVED:

                            if (st.PreApproved) { st.ApprovedById = ticket.RequestedById; }
                            _SecurityService.ApproveTicket(ticket, st.ApprovedById);
                            _hpsmProvider.CloseHpsmTicket(ticket.TicketId);
                            break;
                        case GlobalConstants.HpsmTicketStatus.DENIED: //or Denied?  find out those statuses.

                            _hpsmProvider.CloseHpsmTicket(ticket.TicketId, true);
                            _SecurityService.CloseTicket(ticket, st.RejectedById, st.RejectedReason, st.TicketStatus);
                            break;
                        case GlobalConstants.HpsmTicketStatus.WIDHTDRAWN:

                            _SecurityService.CloseTicket(ticket, st.RejectedById, st.RejectedReason, st.TicketStatus); //Check if the ticket was closed without approval.
                            break;
                        default:
                            break;  //do nothing, we will check again in 15 min.
                    }
                }
                _datasetContext.SaveChanges();
            }

        }

    }
}
