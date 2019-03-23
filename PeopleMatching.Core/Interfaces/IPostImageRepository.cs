using PeopleMatching.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeopleMatching.Core.Interfaces
{
    public interface IPostImageRepository
    {
        void Add(PostImage postImage);
    }
}
