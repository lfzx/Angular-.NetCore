using Microsoft.EntityFrameworkCore;
using PeopleMatching.Core.Entities;
using PeopleMatching.Infrastructure.DataBase.EntityConfigurations;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new PostConfiguration());
            modelBuilder.ApplyConfiguration(new PostImageConfiguration());
        }

        // 将Post弄进来
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
    }
}
