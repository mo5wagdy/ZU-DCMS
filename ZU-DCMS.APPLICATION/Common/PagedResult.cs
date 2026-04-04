using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common
{
    public class PagedResult<T>
    {
        // The list of items for the current page
        public List<T> Items { get; set; } = new();

        // Total number of items across all pages (for pagination metadata)
        public int TotalCount { get; set; }

        // Current page number (1-based index)
        public int Page { get; set; }

        // Number of items per page
        public int PageSize { get; set; }

        // Total number of pages calculated from TotalCount and PageSize
        public int TotalPages =>
            (int)Math.Ceiling(TotalCount / (double)PageSize);

        // Indicates if there is a next page available
        public bool HasNextPage => Page < TotalPages;

        // Indicates if there is a previous page available
        public bool HasPreviousPage => Page > 1;

        // Static factory method to create a PagedResult from a list of items and pagination metadata
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
