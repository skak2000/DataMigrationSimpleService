using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleService.Model;

namespace SimpleService.Configuration
{
    public class SimpleBookConfiguration : IEntityTypeConfiguration<SimpleBook>
    {
        public void Configure(EntityTypeBuilder<SimpleBook> builder)
        {
            builder.ToTable("SimpleBooks");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.PublicId)
                .IsRequired()
                .HasDefaultValueSql("NEWID()");

            builder.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(b => b.Description)
                .HasMaxLength(1000);

            builder.Property(b => b.ISBN)
                .HasMaxLength(50);

            builder.Property(b => b.TraceId)
                .HasMaxLength(100);

            builder.Property(b => b.TenantId)
                .IsRequired();

            builder.Property(b => b.InstanceId)
                .IsRequired();

            builder.Property(b => b.AuthorId)
            .IsRequired();

            builder.HasMany(b => b.Chapters)
                .WithOne(c => c.Book)
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
