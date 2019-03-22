using PeopleMatching.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeopleMatching.Core.Interfaces
{
    public interface IPostRepository
    {
        Task<PaginatedList<Post>> GetAllPostsAsync(PostParameters postParameters);

        Task<Post> GetPostByIdAsync(int id);

        // 添加ADD方法
        void AddPost(Post post);
        void DeletePost(Post post);

        void Update(Post post);  
    }
}
