using System;
using System.Collections.Generic;
using System.Text;

namespace PeopleMatching.Core.Entities
{
    public class Post: Entity
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime LastField { get; set; }

        public string Remark { get; set; }
    }
}
