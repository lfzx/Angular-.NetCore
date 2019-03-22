using Microsoft.Extensions.Logging;
using PeopleMatching.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleMatching.Infrastructure.DataBase
{
    public class MyContextSeed
    {
        public static async Task SeedAsync(MyContext myContext,
            ILoggerFactory loggerFactory, int retry = 0)
        {
            int retryForAvailablity = retry;
            try
            {
                // 如果表里没有任何数据，则存入后面的数据
                if (!myContext.Posts.Any())
                {
                    myContext.Posts.AddRange(
                        new List<Post>
                        {
                            new Post
                            {
                                Title = "Post Title 1",
                                Body = "Post Body 1",
                                Author = "Dave",
                                LastField = DateTime.Now
                            },
                            new Post
                            {
                                 Title = "Post Title 2",
                                Body = "Post Body 2",
                                Author = "Bob",
                                LastField = DateTime.Now
                            },
                             new Post
                            {
                                 Title = "Post Title 3",
                                Body = "Post Body 3",
                                Author = "Marry",
                                LastField = DateTime.Now
                            },
                              new Post
                            {
                                 Title = "Post Title 4",
                                Body = "Post Body 4",
                                Author = "Linda",
                                LastField = DateTime.Now
                            },
                               new Post
                            {
                                 Title = "Post Title 5",
                                Body = "Post Body 5",
                                Author = "Tom",
                                LastField = DateTime.Now
                            },
                                new Post
                            {
                                 Title = "Post Title 6",
                                Body = "Post Body 6",
                                Author = "Jack",
                                LastField = DateTime.Now
                            },
                                 new Post
                            {
                                 Title = "Post Title 7",
                                Body = "Post Body 7",
                                Author = "Json",
                                LastField = DateTime.Now
                            }
                        }
                        );
                    await myContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailablity < 10)
                {
                    retryForAvailablity++;
                    var logger = loggerFactory.CreateLogger<MyContextSeed>();
                    logger.LogError(ex.Message);
                    await SeedAsync(myContext, loggerFactory, retryForAvailablity);
                }
            }
        }
    }
}
