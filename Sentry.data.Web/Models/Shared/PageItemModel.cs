using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web
{
    public class PageItemModel
    {
        public bool IsActive { get; set; }
        public bool IsDisabled { get => PageNumber == Pagination.ELLIPSIS; }
        public string PageNumber { get; set; }
    }
}