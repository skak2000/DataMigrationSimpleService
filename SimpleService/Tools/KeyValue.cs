namespace SimpleService.Tools
{
    public class KeyValue
    {
        public KeyValue(int id, Guid publicId)
        {
            Id = id;
            PublicId = publicId;
        }

        public int Id { get; set; }

        public Guid PublicId { get; set; }
    }
}
