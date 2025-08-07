namespace SimpleService.Request
{
    public class BookRequest
    {
        public Guid AuthorId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ISBN { get; set; }
        public string TraceId { get; set; }
    }
}
