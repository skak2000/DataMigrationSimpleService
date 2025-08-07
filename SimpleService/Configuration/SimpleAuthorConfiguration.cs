using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleService.Model;

namespace SimpleService.Configuration
{
    public class SimpleAuthorConfiguration : IEntityTypeConfiguration<SimpleAuthor>
    {
        public void Configure(EntityTypeBuilder<SimpleAuthor> builder)
        {
            builder.ToTable("SimpleAuthors");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.PublicId)
                .IsRequired()
                .HasDefaultValueSql("NEWID()");

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.TenantId)
                .IsRequired();

            builder.Property(a => a.InstanceId)
                .IsRequired();

            builder.Property(c => c.TraceId)           
                .HasMaxLength(100);

            builder.HasMany(a => a.Books)
                .WithOne(b => b.Author)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
