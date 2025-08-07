using System.ComponentModel.DataAnnotations;

namespace SimpleService.Model
{
    public class SimpleChapter
    {
        [Key]
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string Title { get; set; }
        public int ChapterNumber { get; set; }
        public string Text { get; set; }
        public string TraceId { get; set; }

        public SimpleBook Book { get; set; }
        public Guid TenantId { get; set; }
        public Guid InstanceId { get; set; }
        public int BookId { get; set; }
    }
}
