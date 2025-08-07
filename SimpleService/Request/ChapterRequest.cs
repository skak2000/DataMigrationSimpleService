namespace SimpleService.Request
{
    public class ChapterRequest
    {
        public Guid BookId { get; set; }
        public string Title { get; set; }
        public int ChapterNumber { get; set; }
        public string Text { get; set; }
        public string TraceId { get; set; }
    }
}
