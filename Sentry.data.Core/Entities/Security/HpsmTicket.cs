
namespace Sentry.data.Core
{
    public class HpsmTicket
    {

        public string TicketStatus { get; set; }
        public bool PreApproved { get; set; }
        public string ApprovedById { get; set; }
        public string RejectedById { get; set; }
        public string RejectedReason { get; set; }
    }
}
