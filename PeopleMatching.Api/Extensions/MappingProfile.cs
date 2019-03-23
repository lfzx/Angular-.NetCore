using AutoMapper;
using PeopleMatching.Core.Entities;
using PeopleMatching.Infrastructure.Resources;


namespace PeopleMatching.Api.Extensions
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            //建立了post到PostResource之间的映射
            CreateMap<Post, PostResource>()
             .ForMember(dest => dest.UpdateTime, opt => opt.MapFrom(src => src.LastField));
            ;

            // 添加双向映射
            CreateMap<PostResource, Post>();
            CreateMap<PostAddResource, Post>();
            CreateMap<PostUpdateResource, Post>();

            CreateMap<PostImage, PostImageResource>();
            CreateMap<PostImageResource, PostImage>();

        }
    }
}
