using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleService.Model;

namespace SimpleService.Configuration
{
    public class SimpleChapterConfiguration : IEntityTypeConfiguration<SimpleChapter>
    {
        public void Configure(EntityTypeBuilder<SimpleChapter> builder)
        {
            builder.ToTable("SimpleChapters");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.PublicId)
                .IsRequired()
                .HasDefaultValueSql("NEWID()");

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(c => c.ChapterNumber)
                .IsRequired();

            builder.Property(c => c.Text)
                .HasColumnType("nvarchar(max)");

            builder.Property(c => c.TraceId)
                .HasMaxLength(100);

            builder.Property(c => c.TenantId)
                .IsRequired();

            builder.Property(c => c.InstanceId)
                .IsRequired();

            builder.Property(c => c.BookId) 
                .IsRequired();
        }
    }
}
