namespace OneBeyondApi.Model
{
    public class BorrowerLoans(string borrower, IEnumerable<string> books)
    {
        public string Borrower { get; set; } = borrower;
        public IEnumerable<string> Books { get; set; } = books;
    }
}
