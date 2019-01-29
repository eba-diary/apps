using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.DomainServices
{
    public class SecurityService : ISecurityService
    {

        private readonly IDatasetContext _datasetContext;
        private readonly IHpsmProvider _hpsmProvider;
        private readonly UserService _userService;

        public SecurityService(IDatasetContext datasetContext, IHpsmProvider hpsmProvider, UserService userService)
        {
            _datasetContext = datasetContext;
            _hpsmProvider = hpsmProvider;
            _userService = userService;
        }


        /// <summary>
        /// re-occuring service call that will check if the hpsm ticket has been approved.
        /// </summary>
        public void CheckHpsmTicketStatus()
        {
            List<SecurityTicket> tickets = _datasetContext.HpsmTickets.Where(x => x.TicketStatus == GlobalConstants.HpsmTicketStatus.PENDING).ToList();

            foreach(SecurityTicket ticket in tickets)
            {
                HpsmTicket st = _hpsmProvider.RetrieveTicket(ticket.TicketId);
                switch (st.TicketStatus)
                {
                    case GlobalConstants.HpsmTicketStatus.APPROVED:
                        EnablePermissions(ticket.Id);
                        _hpsmProvider.CloseHpsmTicket(ticket.TicketId);
                        ticket.ApprovedById = st.ApprovedById;
                        ticket.ApprovedDate = DateTime.Now;
                        ticket.TicketStatus = GlobalConstants.HpsmTicketStatus.COMPLETED;
                        break;
                    case GlobalConstants.HpsmTicketStatus.REJECTED: //or Denied?  find out those statuses.
                        _hpsmProvider.CloseHpsmTicket(ticket.TicketId, true);
                        ticket.RejectedById = st.RejectedById;
                        ticket.RejectedDate = DateTime.Now;
                        ticket.TicketStatus = GlobalConstants.HpsmTicketStatus.REJECTED;
                        break;
                    default:
                        break;  //do nothing, we will check again in 15 min.
                }
            }

        }



        public bool RequestPermission(RequestAccess model)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(model.DatasetId);

            model.PrimaryApproverId = ds.PrimaryOwnerId;
            model.SecondaryApproverId = ds.SecondaryOwnerId;
            model.IsProd = Configuration.Config.GetDefaultEnvironmentName().ToUpper() == GlobalConstants.System.PROD;

            string ticketId = _hpsmProvider.CreateHpsmTicket(model);

            SecurityTicket ticket = new SecurityTicket()
            {
                TicketId = ticketId,
                AdGroupName = model.AdGroupName,
                TicketStatus = GlobalConstants.HpsmTicketStatus.PENDING,
                RequestedById = model.RequestorsId,
                RequestedDate = model.RequestedDate,
                IsAddingPermission = true,
                Permissions = new List<SecurityPermission>()
            };

            model.Permissions.ForEach(x => ticket.Permissions.Add(new SecurityPermission(x)));

            _datasetContext.Add(ticket);
            _datasetContext.SaveChanges();

            return true;
        }


        public UserSecurity GetUserSecurity(Security security)
        {
            //if there is no security return null so it breaks. they can not see anything and we should not be calling this if there is not security
            if (security is null){ return null;}

            IApplicationUser user = _userService.GetCurrentUser();

            Dictionary<string, List<SecurityPermission>> dic = security.HpsmTicket.ToDictionary(x => x.AdGroupName, y => y.Permissions.Select(z => z).ToList());

            List<string> userPermissions = new List<string>();

            foreach(var item in dic)
            {
                if (user.IsInGroup(item.Key))
                {
                    userPermissions.AddRange(item.Value.Where(x => x.IsEnabled).Select(x => x.Permission.PermissionCode).ToList());
                }
            }

            return new UserSecurity()
            {
                CanPreviewDataset = userPermissions.Contains(GlobalConstants.SecurityPermissions.CAN_PREVIEW_DATASET),
                CanViewFullDataset = userPermissions.Contains(GlobalConstants.SecurityPermissions.CAN_VIEW_FULL_DATASET),
                CanQueryDataset = userPermissions.Contains(GlobalConstants.SecurityPermissions.CAN_QUERY_DATASET),
                CanConnectToDataset = userPermissions.Contains(GlobalConstants.SecurityPermissions.CAN_CONNECT_TO_DATASET),
                CanUploadToDataset = userPermissions.Contains(GlobalConstants.SecurityPermissions.CAN_UPLOAD_TO_DATASET)
            };
        }


        private void EnablePermissions(Guid hpsmTicketId)
        {
            SecurityTicket ticket = _datasetContext.GetById<SecurityTicket>(hpsmTicketId);

            ticket.Permissions.ForEach(x =>
            {
                x.IsEnabled = true;
                x.EnabledDate = DateTime.Now;
            });

            _datasetContext.SaveChanges();
        }

        public void RemovePermissions(Guid hpsmTicketId)
        {
            SecurityTicket ticket = _datasetContext.GetById<SecurityTicket>(hpsmTicketId);

            ticket.Permissions.ForEach(x =>
            {
                x.IsEnabled = false;
                x.RemovedDate = DateTime.Now;
            });

            _datasetContext.SaveChanges();
        }






    }
}
