using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeopleMatching.Core.Entities;

namespace PeopleMatching.Infrastructure.DataBase.EntityConfigurations
{
    public class PostImageConfiguration : IEntityTypeConfiguration<PostImage>
    {
        public void Configure(EntityTypeBuilder<PostImage> builder)
        {
            builder.Property(x => x.FileName).IsRequired().HasMaxLength(100);
        }
    }
}
