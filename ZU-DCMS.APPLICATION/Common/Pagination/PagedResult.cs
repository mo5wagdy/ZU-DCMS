namespace ZU_DCMS.APPLICATION.Common.Pagination
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new(); // => The list of items for the current page
        public int TotalCount { get; set; } // => Total number of items across all pages (for pagination metadata)
        public int Page { get; set; } // => Current page number (1-based index)
        public int PageSize { get; set; } // => Number of items per page
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize); // => Total number of pages calculated from TotalCount and PageSize
        public bool HasNextPage => Page < TotalPages; // => Indicates if there is a next page available
        public bool HasPreviousPage => Page > 1; // => Indicates if there is a previous page available


        // __ Static factory method to create a PagedResult from a list of items and pagination metadata __ //
        public static PagedResult<T> Create(List<T> items, int totalCount, PagedRequest request)
        {
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
