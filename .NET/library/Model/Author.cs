namespace OneBeyondApi.Model
{
    public class Author(string name)
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = name;
    }
}
