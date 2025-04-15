namespace OneBeyondApi.Model
{
    public class BookStock
    {
        public BookStock()
        {

        }

        public BookStock(Book book)
        {
            Book = book ?? throw new ArgumentNullException(nameof(book));
        }

        public Guid Id { get; set; }
        public Book Book { get; set; }
        public DateTime? LoanEndDate { get; set; }
        public Borrower? OnLoanTo { get; set; }
    }
}
