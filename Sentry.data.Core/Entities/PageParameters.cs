namespace Sentry.data.Core
{
    public class PageParameters
    {
        const int maxPageSize = 100;
        private int? _pageNumber = 1;
        public int? PageNumber
        {
            get
            {
                return _pageNumber;
            }
            set
            {
                if(value != null)
                {
                    _pageNumber = value;
                }
            }
        }
        
        private int? _pageSize = 10;
        public int? PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value != null)
                {
                    _pageSize = (value > maxPageSize) ? maxPageSize : value;
                }                
            }
        }
    }
}