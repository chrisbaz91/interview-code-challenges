using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class CatalogueRepository(LibraryContext context) : ICatalogueRepository
    {
        public List<BookStock> GetCatalogue()
        {
            var list = context.Catalogue
                .Include(x => x.Book)
                .ThenInclude(x => x.Author)
                .Include(x => x.OnLoanTo)
                .ToList();
            return list;
        }

        public async Task<IEnumerable<BorrowerLoans>> GetLoans()
        {
            var list = await context.Catalogue
                .Include(x => x.Book)
                .ThenInclude(x => x.Author)
                .Include(x => x.OnLoanTo)
                .Where(x => x.LoanEndDate != null && x.OnLoanTo != null)
                .ToListAsync();

            return list.Select(x =>
                new BorrowerLoans(x.OnLoanTo.Name,
                    list.Where(y => y.OnLoanTo == x.OnLoanTo)
                        .Select(z => z.Book.Name)
                    )
                )
                .DistinctBy(x => x.Borrower);
        }

        public async Task<string> ReturnBook(LoanRequest request)
        {
            var baseFine = 3;
            var dailyFineRate = 0.75;
            var resultMessage = "Book successfully returned, thank you!";

            var borrower = await context.Borrowers.FirstOrDefaultAsync(x => x.Name == request.Borrower);

            if (borrower == null)
            {
                return "Error finding borrower, please try again.";
            }

            var bookStocks = await SearchCatalogue(new CatalogueSearch(request.BookName, request.Author));

            if (bookStocks == null || !bookStocks.Any())
            {
                return "Error finding book, please try again.";
            }

            var bookStock = await context.Catalogue
                .Include(x => x.OnLoanTo)
                .SingleOrDefaultAsync(x => x.OnLoanTo == borrower);

            if (bookStock != null && bookStock.LoanEndDate != null)
            {
                if (bookStock.LoanEndDate < DateTime.Today)
                {
                    var daysOverdue = (DateTime.Today - bookStock.LoanEndDate).Value.Days;
                    var totalFine = baseFine + daysOverdue * dailyFineRate;

                    bookStock.OnLoanTo.FinesOwed += totalFine;

                    resultMessage +=
                        $" - due to being {daysOverdue} days overdue, " +
                        $"you owe {totalFine:C}, " +
                        $"your new fines total is {bookStock.OnLoanTo.FinesOwed:C}.";
                }

                bookStock.LoanEndDate = null;
                bookStock.OnLoanTo = null;
                await context.SaveChangesAsync();

                return resultMessage;
            }

            return "Error finding loan, please try again.";
        }

        public async Task<string> LoanBook(LoanRequest request)
        {
            var resultMessage = "Book successfully loaned, thank you!";

            var borrower = await context.Borrowers.FirstOrDefaultAsync(x => x.Name == request.Borrower);

            if (borrower == null)
            {
                return "Error finding borrower, please try again.";
            }

            var bookStocks = await SearchCatalogue(new CatalogueSearch(request.BookName, request.Author));

            if (bookStocks == null || !bookStocks.Any())
            {
                return "Error finding book, please try again.";
            }

            var book = await context.Books.SingleAsync(x => x.Id == bookStocks.First().Book.Id);

            var availableBookStock = bookStocks.FirstOrDefault(x => x.LoanEndDate == null && x.OnLoanTo == null);

            if (availableBookStock != null)
            {
                var currentBookStock = await context.Catalogue.SingleAsync(x => x.Id == availableBookStock.Id);
                currentBookStock.LoanEndDate = DateTime.Now.AddDays(7);
                currentBookStock.OnLoanTo = borrower;

                var currentReservation = await context.Reservations.SingleOrDefaultAsync(x => x.ReservedBy == borrower && x.Book == book);
                if (currentReservation != null)
                {
                    context.Remove(currentReservation);
                }
                await context.SaveChangesAsync();
            }
            else
            {
                var reservation = new Reservation(book.Id, borrower.Id, DateTime.Now);
                await context.Reservations.AddAsync(reservation);
                await context.SaveChangesAsync();

                var currentReservations = await context.Reservations
                    .CountAsync(x => x.BookId == book.Id);

                resultMessage = "There are no copies of this book currently available, " +
                    "however a reservation has been made - " +
                    $"you are position {currentReservations} in the queue.";
            }

            return resultMessage;
        }

        public async Task<string> GetAvailability(LoanRequest request)
        {
            var borrower = await context.Borrowers.FirstOrDefaultAsync(x => x.Name == request.Borrower);

            if (borrower == null)
            {
                return "Error finding borrower, please try again.";
            }

            var bookStocks = (await SearchCatalogue(new CatalogueSearch(request.BookName, request.Author)))
                            .OrderBy(x => x.LoanEndDate);

            if (bookStocks == null || !bookStocks.Any())
            {
                return "Error finding book, please try again.";
            }

            var reservations = await context.Reservations
                .Include(x => x.Book)
                .Include(x => x.ReservedBy)
                .Where(x => x.Book.Name == request.BookName)
                .ToListAsync();

            var currentReservation = reservations.FirstOrDefault(x => x.ReservedBy.Name == request.Borrower);

            if (currentReservation == null)
            {
                return "Error finding reservation, please try again.";
            }

            var queuePosition = reservations.Count(x => x.ReservedDate <= currentReservation.ReservedDate);
            DateTime? loanEndDate = null;
            var availableDate = new DateTime();
            var initialDaysToWait = 0;
            var reservedDaysToWait = 0;
            var totalDaysToWait = 0;

            if (bookStocks.Count() == 1)
            {
                // queuePosition-1 to account for next reservations being able to loan at the previous loan end date
                loanEndDate = bookStocks.Single().LoanEndDate;
                reservedDaysToWait = 7 * (queuePosition - 1);
            }
            else
            {
                // two queues for two stocks
                var earlierBookStockDate = bookStocks.First().LoanEndDate;
                var laterBookStockDate = bookStocks.Last().LoanEndDate;
                // odd position gets earlier stock, even position gets later stock
                loanEndDate = queuePosition % 2 != 0 ? earlierBookStockDate : laterBookStockDate;
                // half queue position rounded up to get single queue position
                reservedDaysToWait = 7 * ((int)Math.Ceiling((double)queuePosition / 2) - 1);
            }

            initialDaysToWait = loanEndDate.HasValue && loanEndDate.Value > DateTime.Now.Date ? (loanEndDate.Value - DateTime.Now.Date).Days : 0;
            totalDaysToWait = initialDaysToWait + reservedDaysToWait;
            availableDate = DateTime.Now.Date.AddDays(totalDaysToWait);

            return $"This book will be available for you " +
                $"{(totalDaysToWait > 0
                ? $"on {availableDate.Date.ToShortDateString()} which is {totalDaysToWait} days away!"
                : "today!")}";
        }

        public async Task<List<BookStock>> SearchCatalogue(CatalogueSearch search)
        {
            var list = context.Catalogue
                .Include(x => x.Book)
                .ThenInclude(x => x.Author)
                .Include(x => x.OnLoanTo)
                .AsQueryable();

            if (search != null)
            {
                if (!string.IsNullOrEmpty(search.Author))
                {
                    list = list.Where(x => x.Book.Author.Name.Contains(search.Author));
                }
                if (!string.IsNullOrEmpty(search.BookName))
                {
                    list = list.Where(x => x.Book.Name.Contains(search.BookName));
                }
            }

            return await list.ToListAsync();
        }
    }
}
