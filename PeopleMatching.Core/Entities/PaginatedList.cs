using System;
using System.Collections.Generic;
using System.Text;

namespace PeopleMatching.Core.Entities
{
    public class PaginatedList<T> : List<T> where T : class
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }

        private int _totalItemCount;
        public int TotalItemsCount
        {
            get => _totalItemCount;
            set => _totalItemCount = value >= 0 ? value : 0;

        }

        public int PageCount => TotalItemsCount / PageSize + (TotalItemsCount % PageSize > 0 ? 1 : 0);

        // 是否有前一页HasPrevious， HasNext是否有后一页
        public bool HasPrevious => PageIndex > 0;
        public bool HasNext => PageIndex < PageCount - 1;

        public PaginatedList(int pageIndex, int pageSize, int totalItemsCount, IEnumerable<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalItemsCount = totalItemsCount;
            AddRange(data);
        }
    }
}
