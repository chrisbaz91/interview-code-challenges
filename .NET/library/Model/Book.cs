namespace OneBeyondApi.Model
{
    public class Book
    {
        public Book()
        {

        }

        public Book(string name, Author author, BookFormat format, string isbn)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(isbn))
            {
                throw new ArgumentException($"'{nameof(isbn)}' cannot be null or whitespace.", nameof(isbn));
            }

            Name = name;
            ISBN = isbn;
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Format = format;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Author Author { get; set; }
        public BookFormat Format { get; set; }
        public string ISBN { get; set; }
    }
}
