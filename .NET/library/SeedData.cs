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

            context.Authors.Add(ernestMonkjack);
            context.Authors.Add(sarahKennedy);
            context.Authors.Add(margaretJones);
            context.Authors.Add(markMillar);
            context.Authors.Add(brianMichaelBendis);
            context.Authors.Add(jMStraczynski);
            context.Authors.Add(johnathanHickman);

            context.Books.Add(clayBook);
            context.Books.Add(agileBook);
            context.Books.Add(rustBook);
            context.Books.Add(civilWarBook);
            context.Books.Add(avxBook);
            context.Books.Add(spiderManBook);
            context.Books.Add(infinityBook);

            context.Borrowers.Add(daveSmith);
            context.Borrowers.Add(lianaJames);
            context.Borrowers.Add(chrisBarrett);

            context.Catalogue.Add(bookOnLoanUntilToday);
            context.Catalogue.Add(bookNotOnLoan);
            context.Catalogue.Add(bookOnLoanUntilNextWeek);
            context.Catalogue.Add(rustBookStock);
            context.Catalogue.Add(civilWarBookStock);
            context.Catalogue.Add(avxBookStock);
            context.Catalogue.Add(avxBookStock2);
            context.Catalogue.Add(spiderManBookStock);
            context.Catalogue.Add(infinityBookStock);

            await context.SaveChangesAsync();
        }
    }
}
