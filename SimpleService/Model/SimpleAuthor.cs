using System.ComponentModel.DataAnnotations;

namespace SimpleService.Model
{
    public class SimpleAuthor
    {
        [Key]
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string Name { get; set; }        
        public Guid TenantId { get; set; }
        public Guid InstanceId { get; set; }
        public string TraceId { get; set; }
        public List<SimpleBook> Books { get; set; } 
    }
}
