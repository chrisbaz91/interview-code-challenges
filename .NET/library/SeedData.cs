using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApi
{
    public class SeedData
    {
        public static void SetInitialData()
        {
            var ernestMonkjack = new Author("Ernest Monkjack");
            var sarahKennedy = new Author("Sarah Kennedy");
            var margaretJones = new Author("Margaret Jones");

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

            using var context = new LibraryContext();

                context.Authors.Add(ernestMonkjack);
                context.Authors.Add(sarahKennedy);
                context.Authors.Add(margaretJones);


                context.Books.Add(clayBook);
                context.Books.Add(agileBook);
                context.Books.Add(rustBook);

                context.Borrowers.Add(daveSmith);
                context.Borrowers.Add(lianaJames);

                context.Catalogue.Add(bookOnLoanUntilToday);
                context.Catalogue.Add(bookNotOnLoan);
                context.Catalogue.Add(bookOnLoanUntilNextWeek);
                context.Catalogue.Add(rustBookStock);

                context.SaveChanges();
        }
    }
}
