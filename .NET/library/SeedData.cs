using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApi
{
    public class SeedData
    {
        public static async Task SetInitialData()
        {
            var ernestMonkjack = new Author("Ernest Monkjack");
            var sarahKennedy = new Author("Sarah Kennedy");
            var margaretJones = new Author("Margaret Jones");
            var markMillar = new Author("Mark Millar");
            var brianMichaelBendis = new Author("Brian Michael Bendis");
            var jMStraczynski = new Author("J M Straczynski");
            var johnathanHickman = new Author("Johnathan Hickman");

            var clayBook = new Book
            (
                "The Importance of Clay",
                ernestMonkjack,
                BookFormat.Paperback,
                "1305718181"
            );

            var agileBook = new Book
            (
                "Agile Project Management - A Primer",
                sarahKennedy,
                BookFormat.Hardback,
                "1293910102"
            );

            var rustBook = new Book
            (
                "Rust Development Cookbook",
                margaretJones,
                BookFormat.Paperback,
                "3134324111"
            );

            var civilWarBook = new Book
            (
                "Marvel Civil War",
                markMillar,
                BookFormat.GraphicNovel,
                "9781905239603"
            );

            var avxBook = new Book
            (
                "Avengers vs. X-Men",
                brianMichaelBendis,
                BookFormat.GraphicNovel,
                "9780785138938"
            );

            var spiderManBook = new Book
            (
                "Spider-Man Ultimate Collection Vol. 1",
                jMStraczynski,
                BookFormat.GraphicNovel,
                "9780785163176"
            );

            var infinityBook = new Book
            (
                "Marvel Infinity",
                johnathanHickman,
                BookFormat.GraphicNovel,
                "9780785184232"
            );

            var daveSmith = new Borrower
            (
                "Dave Smith",
                "dave@smithy.com"
            );

            var lianaJames = new Borrower
            (
                "Liana James",
                "liana@gmail.com"
            );

            var chrisBarrett = new Borrower
            (
                "Chris Barrett",
                "cpbarrett91@gmail.com"
            )
            {
                FinesOwed = 2.25
            };

            var bookOnLoanUntilToday = new BookStock(clayBook)
            {
                OnLoanTo = daveSmith,
                LoanEndDate = DateTime.Now.Date
            };

            var bookNotOnLoan = new BookStock(clayBook)
            {
                OnLoanTo = null,
                LoanEndDate = null
            };

            var bookOnLoanUntilNextWeek = new BookStock(agileBook)
            {
                OnLoanTo = lianaJames,
                LoanEndDate = DateTime.Now.Date.AddDays(7)
            };

            var rustBookStock = new BookStock(rustBook)
            {
                OnLoanTo = null,
                LoanEndDate = null
            };

            var civilWarBookStock = new BookStock(civilWarBook)
            {
                OnLoanTo = chrisBarrett,
                LoanEndDate = DateTime.Now.Date.AddDays(-7)
            };

            var avxBookStock = new BookStock(avxBook)
            {
                OnLoanTo = null,
                LoanEndDate = null
            };

            var avxBookStock2 = new BookStock(avxBook)
            {
                OnLoanTo = chrisBarrett,
                LoanEndDate = DateTime.Now.Date.AddDays(-7)
            };

            var spiderManBookStock = new BookStock(spiderManBook)
            {
                OnLoanTo = null,
                LoanEndDate = null
            };

            var infinityBookStock = new BookStock(infinityBook)
            {
                OnLoanTo = null,
                LoanEndDate = null
            };

            using var context = new LibraryContext();

            await context.Authors.AddRangeAsync(ernestMonkjack, sarahKennedy, margaretJones, markMillar, brianMichaelBendis, jMStraczynski, johnathanHickman);

            await context.Books.AddRangeAsync(clayBook, agileBook, rustBook,  civilWarBook, avxBook, spiderManBook, infinityBook);

            await context.Borrowers.AddRangeAsync(daveSmith, lianaJames, chrisBarrett);

            await context.Catalogue.AddRangeAsync(bookOnLoanUntilToday, bookNotOnLoan,  bookNotOnLoan, rustBookStock, civilWarBookStock, avxBookStock, avxBookStock2, spiderManBookStock, infinityBookStock);

            await context.SaveChangesAsync();
        }
    }
}
