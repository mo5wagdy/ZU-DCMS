using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Common.Pagination
{
    public class PagedRequest
    {
        // __ Default values for pagination parameters __ //
        private int _page = 1;
        private int _pageSize = 10;

        // __ Properties with validation in setters to ensure valid pagination parameters __ //
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 50 ? 50 : value < 1 ? 10 : value;
        }

        // __ Optional sorting and searching parameters __ //
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public string? SearchTerm { get; set; }
    }
}
