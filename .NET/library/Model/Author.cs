namespace OneBeyondApi.Model
{
    public class Author(string name) : Entity
    {
        public string Name { get; set; } = name;
    }
}
