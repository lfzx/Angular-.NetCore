using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PeopleMatching.Api.Helper;
using PeopleMatching.Core.Entities;
using PeopleMatching.Core.Interfaces;
using PeopleMatching.Infrastructure.DataBase;
using PeopleMatching.Infrastructure.Extensions;
using PeopleMatching.Infrastructure.Resources;
using PeopleMatching.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleMatching.Api.Controllers
{
    [Route("api/posts")]
    public class PostController: Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUrlHelper _urlHelper;
        private readonly IMapper _mapper;
        private readonly ITypeHelperService _typeHelperService;
        private readonly IPropertyMappingContainer _propertyMappingContainer;
        private readonly ILogger _logger;

        // 将mycontext注入到构造函数中使用
        public PostController(
            IPostRepository postRepository,
            IUnitOfWork unitOfWork,
            ILoggerFactory loggerFactory,
            IUrlHelper urlHelper,
            IMapper mapper,
            ITypeHelperService typeHelperService,
            IPropertyMappingContainer propertyMappingContainer
            )
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _urlHelper = urlHelper;
            _mapper = mapper;
            _typeHelperService = typeHelperService;
            _propertyMappingContainer = propertyMappingContainer;
            _logger = loggerFactory.CreateLogger("PeopleMatching.Api.Controller.PostController");
        }

        [AllowAnonymous]
        [HttpGet(Name = "GetPosts")]
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/vnd.cgzl.hateoas+json" })]
        public async Task<IActionResult> GetHateoas(PostParameters postParameters)
        {
            // 判断排序映射
            if (!_propertyMappingContainer.ValidateMappingExistsFor<PostResource, Post>(postParameters.OrderBy))
            {
                return BadRequest("Can't finds fields for sorting.");
            }
            // 判断fields是否在元数据中存在
            if (!_typeHelperService.TypeHasProperties<PostResource>(postParameters.Fields))
            {
                //  返回400错误
                return BadRequest("Fields not exist.");
            }

            // 读取数据库中是否有数据
            var postList = await _postRepository.GetAllPostsAsync(postParameters);
            var postResources = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);

            var shapedPostResources = postResources.ToDynamicIEnumerable(postParameters.Fields);
            var shapedWithLinks = shapedPostResources.Select(x =>
            {
                // 转型
                var dict = x as IDictionary<string, object>;
                var postLinks = CreateLinksForPost((int)dict["Id"], postParameters.Fields);
                dict.Add("links", postLinks);
                return dict;
            });
            var links = CreateLinksForPosts(postParameters, postList.HasPrevious, postList.HasNext);
            var result = new
            {
                value = shapedWithLinks,
                links
            };

            //翻页元数据
            var meta = new
            {
                postList.PageSize,
                postList.PageIndex,
                postList.TotalItemsCount,
                postList.PageCount
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet(Name = "GetPosts")]
        public async Task<IActionResult> Get(PostParameters postParameters)
        {
            if (!_propertyMappingContainer.ValidateMappingExistsFor<PostResource, Post>(postParameters.OrderBy))
            {
                return BadRequest("Can't finds fields for sorting.");
            }

            if (!_typeHelperService.TypeHasProperties<PostResource>(postParameters.Fields))
            {
                return BadRequest("Fields not exist.");
            }

            var postList = await _postRepository.GetAllPostsAsync(postParameters);

            var postResources = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);
            //如果有前一页则生成前一页链接，无则null
            var previousPageLink = postList.HasPrevious ?
                CreatePostUri(postParameters,
                    PaginationResourceUriType.PreviousPage) : null;
            //生成后一页链接
            var nextPageLink = postList.HasNext ?
                CreatePostUri(postParameters,
                    PaginationResourceUriType.NextPage) : null;

            var meta = new
            {
                postList.TotalItemsCount,
                postList.PageSize,
                postList.PageIndex,
                postList.PageCount,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            return Ok(postResources.ToDynamicIEnumerable(postParameters.Fields));
        }

        [AllowAnonymous]
        [HttpGet("{id}",Name ="GetPost")]
        public async Task<IActionResult> Get(int id,string fields)
        {
            if (!_typeHelperService.TypeHasProperties<PostResource>(fields))
            {
                return BadRequest("Fields not exist.");
            }

            var post = await _postRepository.GetPostByIdAsync(id);

            if (post == null)
            {
                //返回404
                return NotFound();
            }

            var postResource = _mapper.Map<Post, PostResource>(post);

            //对元数据进行塑形
            var shapedPostResource = postResource.ToDynamic(fields);

            var links = CreateLinksForPost(id, fields);
            var result = (IDictionary<string, object>)shapedPostResource;
            result.Add("links", links);

            return Ok(result);

        }


        [HttpPost(Name = "CreatePost")]
        [RequestHeaderMatchingMediaType("Content-Type", new[] { "application/vnd.cgzl.post.create+json" })]
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/vnd.cgzl.hateoas+json" })]
        public async Task<IActionResult> Post([FromBody] PostAddResource postAddResource)
        {
            if (postAddResource == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState); 
            }

            var newPost = _mapper.Map<PostAddResource, Post>(postAddResource);

            newPost.Author = "admin";
            newPost.LastField = DateTime.Now;

            _postRepository.AddPost(newPost);

            // 判断是否保存失败
            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception("Save Failed!");
            }

            var resultResource = _mapper.Map<Post, PostResource>(newPost);
            //获得链接
            var links = CreateLinksForPost(newPost.Id);
            var linkedPostResource = resultResource.ToDynamic() as IDictionary<string, object>;
            linkedPostResource.Add("links", links);

            //创建资源后，如何获取该资源的uri，第一个参数是获取单个对象的action名，getaction的参数是匿名类，
            // 第三个参数是创建好的资源
            return CreatedAtRoute("GetPost", new { id = linkedPostResource["Id"] }, linkedPostResource);
        }

        [HttpDelete("{id}", Name = "DeletePost")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            _postRepository.DeletePost(post);

            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception($"Deleting post {id} failed when saving.");
            }

            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdatePost")]
        [RequestHeaderMatchingMediaType("Content-Type", new[] { "application/vnd.cgzl.post.update+json" })]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] PostUpdateResource postUpdate)
        {
            if (postUpdate == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }

            // 从数据库把该对象查出来
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // 服务器端更新修改时间
            post.LastField = DateTime.Now;
            // 使用mapper把传进来的参数PostUpdateResource的属性映射到post上，相当于整体更新这些属性
            _mapper.Map(postUpdate, post);

            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception($"Updating post {id} failed when saving.");
            }
            return NoContent();
        }

        [HttpPatch("{id}", Name = "PartiallyUpdatePost")]
        public async Task<IActionResult> PartiallyUpdateCityForCountry(int id,
            [FromBody] JsonPatchDocument<PostUpdateResource> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // 把取出来的对象映射到PostUpdateResource类型上
            var postToPatch = _mapper.Map<PostUpdateResource>(post);
            // 将传入的修改参数附到postToPatch上，并传入ModelState（用于验证）
            patchDoc.ApplyTo(postToPatch, ModelState);

            TryValidateModel(postToPatch);

            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }

            _mapper.Map(postToPatch, post);
            post.LastField = DateTime.Now;
            _postRepository.Update(post);

            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception($"Patching city {id} failed when saving.");
            }

            return NoContent();
        }


        //建立创建uri的方法,把post这个查询参数传进来，判断要生成前一页还是后一页的链接
        private string CreatePostUri(PostParameters parameters, PaginationResourceUriType uriType)
        {
            switch (uriType)
            {
                // 把查询参数parameters重新赋值，做新的参数,最后返回 匹配Link中第一个参数值的action
                case PaginationResourceUriType.PreviousPage:
                    var previousParameters = new
                    {
                        pageIndex = parameters.PageIndex - 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields,
                        title = parameters.Title
                    };
                    return _urlHelper.Link("GetPosts", previousParameters);
                case PaginationResourceUriType.NextPage:
                    var nextParameters = new
                    {
                        pageIndex = parameters.PageIndex + 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields,
                        title = parameters.Title
                    };
                    return _urlHelper.Link("GetPosts", nextParameters);
                default:
                    var currentParameters = new
                    {
                        pageIndex = parameters.PageIndex,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields,
                        title = parameters.Title
                    };
                    return _urlHelper.Link("GetPosts", currentParameters);
            }
        }

        // 为每个post资源创建与其相关的链接
        private IEnumerable<LinkResource> CreateLinksForPost(int id, string fields = null)
        {
            var links = new List<LinkResource>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkResource(
                        _urlHelper.Link("GetPost", new { id }), "self", "GET"));
            }
            else
            {
                links.Add(
                    new LinkResource(
                        _urlHelper.Link("GetPost", new { id, fields }), "self", "GET"));
            }

            links.Add(
                new LinkResource(
                    _urlHelper.Link("DeletePost", new { id }), "delete_post", "DELETE"));

            return links;
        }

        private IEnumerable<LinkResource> CreateLinksForPosts(PostParameters postResourceParameters,
            bool hasPrevious, bool hasNext)
        {
            var links = new List<LinkResource>
            {
                new LinkResource(
                    CreatePostUri(postResourceParameters, PaginationResourceUriType.CurrentPage),
                    "self", "GET")
            };

            if (hasPrevious)
            {
                links.Add(
                    new LinkResource(
                        CreatePostUri(postResourceParameters, PaginationResourceUriType.PreviousPage),
                        "previous_page", "GET"));
            }

            if (hasNext)
            {
                links.Add(
                    new LinkResource(
                        CreatePostUri(postResourceParameters, PaginationResourceUriType.NextPage),
                        "next_page", "GET"));
            }

            return links;
        }


    }
}
