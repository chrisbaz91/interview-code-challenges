namespace OneBeyondApi.Model
{
    public class BookStock : Entity
    {
        public BookStock()
        {

        }

        public BookStock(Book book)
        {
            Book = book ?? throw new ArgumentNullException(nameof(book));
        }

        public Book Book { get; set; }
        public DateTime? LoanEndDate { get; set; }
        public Borrower? OnLoanTo { get; set; }
    }
}
