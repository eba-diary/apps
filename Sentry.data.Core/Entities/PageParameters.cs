namespace Sentry.data.Core
{
    public class PageParameters
    {
        public PageParameters(int? pageNumber, int? pageSize)
        {
            _pageNumber = pageNumber ?? 1;
            if (pageSize == null || pageSize == 0)
            {
                _pageSize = defaultPageSize;
            }
            else if (pageSize > maxPageSize)
            {
                _pageSize = maxPageSize;
            }
            else
            {
                _pageSize = pageSize ?? defaultPageSize;
            }
        }
        const int maxPageSize = 10000;
        const int defaultPageSize = 10;
        private readonly int _pageSize;
        private readonly int _pageNumber;

        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
        }
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
        }
    }
}