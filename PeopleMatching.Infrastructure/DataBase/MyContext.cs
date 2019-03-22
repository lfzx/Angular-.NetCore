using Microsoft.EntityFrameworkCore;
using PeopleMatching.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeopleMatching.Infrastructure.DataBase
{
    public class MyContext:DbContext
    {
        //构造函数
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {

        }

        // 将Post弄进来
        public DbSet<Post> Posts { get; set; }
    }
}
