using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleMatching.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeopleMatching.Infrastructure.DataBase.EntityConfigurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            // 做约束
            builder.Property(x => x.Remark).HasMaxLength(200);

        }
    }
}
