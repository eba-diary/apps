namespace Sentry.data.Core.Helpers.Paginate
{
    public class PageParameters
    {
        public PageParameters(int? pageNumber, int? pageSize, bool SortDesc = false) // SortDesc has a default value of false
        {
            _pageNumber = pageNumber.GetValueOrDefault() <= 0 ? 1 : pageNumber.Value;
            if (pageSize.GetValueOrDefault() <= 0)
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
            _sortDesc = SortDesc; 
            
        }
        const int maxPageSize = 10000;
        const int defaultPageSize = 10;
        private readonly int _pageSize;
        private readonly int _pageNumber;

        // private instance variable for sorting by descending or ascending
        private readonly bool _sortDesc; 

        /*
         * Attribute that gets the pageNumber of the PageParameter object
         */
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
        }

        /*
         * Number of records you want to take
         */
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
        }

        /*
         * Determines whether to get the data is ascending or descending order
         */
        public bool SortDesc 
        {
            get
            {
                return _sortDesc;
            }
        }
    }
}