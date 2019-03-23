using PeopleMatching.Core.Entities;
using PeopleMatching.Core.Interfaces;
using PeopleMatching.Infrastructure.DataBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeopleMatching.Infrastructure.Repositories
{
    public class PostImageRepository : IPostImageRepository
    {
        private readonly MyContext _myContext;

        public PostImageRepository(MyContext myContext)
        {
            _myContext = myContext;
        }

        public void Add(PostImage postImage)
        {
            _myContext.PostImages.Add(postImage);
        }

    }
}
