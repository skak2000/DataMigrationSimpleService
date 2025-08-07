using System.ComponentModel.DataAnnotations;

namespace SimpleService.Model
{
    public class SimpleBook
    {
        [Key]
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string Title { get; set; }           
        public string Description { get; set; }

        // ISBN can be use to ID the book
        public string ISBN { get; set; }
        public string TraceId { get; set; }
        public Guid TenantId { get; set; }
        public Guid InstanceId { get; set; }

        public SimpleAuthor Author { get; set; }
        public List<SimpleChapter> Chapters { get; set; }
        public int AuthorId { get; set; }
    }
}
