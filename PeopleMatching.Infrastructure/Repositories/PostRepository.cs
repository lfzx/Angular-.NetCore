using Microsoft.EntityFrameworkCore;
using PeopleMatching.Core.Entities;
using PeopleMatching.Core.Interfaces;
using PeopleMatching.Infrastructure.DataBase;
using PeopleMatching.Infrastructure.Extensions;
using PeopleMatching.Infrastructure.Resources;
using PeopleMatching.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleMatching.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly MyContext _myContext;
        private readonly IPropertyMappingContainer _propertyMappingContainer;

        public PostRepository(
            MyContext myContext,
            IPropertyMappingContainer propertyMappingContainer)
        {
            _myContext = myContext;
            _propertyMappingContainer = propertyMappingContainer;
        }

        public void AddPost(Post post)
        {
            _myContext.Posts.Add(post);
        }

        public void DeletePost(Post post)
        {
            _myContext.Posts.Remove(post);
        } 

        public async Task<PaginatedList<Post>> GetAllPostsAsync(PostParameters postParameters)
        {
            var query = _myContext.Posts.AsQueryable();

            if (!string.IsNullOrEmpty(postParameters.Title))
            {
                var title = postParameters.Title.ToLowerInvariant();
                query = query.Where(x => x.Title.ToLowerInvariant() == title);
            }

            query = query.ApplySort(postParameters.OrderBy, _propertyMappingContainer.Resolve<PostResource, Post>());

            var count = await query.CountAsync();
            var data = await query
                .Skip(postParameters.PageIndex * postParameters.PageSize)
                .Take(postParameters.PageSize)
                .ToListAsync();

            return new PaginatedList<Post>(postParameters.PageIndex, postParameters.PageSize, count, data);
        }

        // 查询
        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _myContext.Posts.FindAsync(id);
        }

        public void Update(Post post)
        {
            //把post Entity设置为Modified
            _myContext.Entry(post).State = EntityState.Modified;
        }
    }
}
