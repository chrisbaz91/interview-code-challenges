namespace OneBeyondApi.Model
{
    public class CatalogueSearch(string bookName, string author)
    {
        public string BookName { get; set; } = bookName;
        public string Author { get; set; } = author;
    }
}
