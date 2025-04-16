namespace OneBeyondApi.Model
{
    public class LoanRequest(string bookName, string author, string borrower)
    {
        public string Borrower { get; set; } = borrower;
        public string BookName { get; set; } = bookName;
        public string Author { get; set; } = author;
    }
}
