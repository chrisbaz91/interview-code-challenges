using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;
using System.Linq;

namespace OneBeyondApi.DataAccess
{
    public class CatalogueRepository : ICatalogueRepository
    {
        private readonly LibraryContext _context;

        public CatalogueRepository(LibraryContext context)
        {
            _context = context;
        }

        public List<BookStock> GetCatalogue()
        {
            var list = _context.Catalogue
                .Include(x => x.Book)
                .ThenInclude(x => x.Author)
                .Include(x => x.OnLoanTo)
                .ToList();
            return list;
        }

        public async Task<IEnumerable<BorrowerLoans>> GetLoans()
        {
            var list = await _context.Catalogue
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

        public async Task<string> ReturnBook(Guid guid)
        {
            var baseFine = 3;
            var dailyFineRate = 0.75;
            var resultMessage = "Book successfully returned, thank you!";

            var bookStock = await _context.Catalogue
                .Include(x => x.OnLoanTo)
                .Where(x => x.Id == guid)
                .SingleOrDefaultAsync();

            if (bookStock != null
                && bookStock.LoanEndDate != null
                && bookStock.OnLoanTo != null)
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
                await _context.SaveChangesAsync();

                return resultMessage;
            }

            return "Error finding loan, please try again.";
        }

        public async Task<string> LoanBook(LoanRequest request)
        {
            var resultMessage = "Book successfully loaned, thank you!";

            var borrower = await _context.Borrowers.FirstOrDefaultAsync(x => x.Name == request.Borrower);

            if (borrower == null)
            {
                return "Error finding borrower, please try again.";
            }

            var bookStocks = await SearchCatalogue(new CatalogueSearch(request.BookName, request.Author));

            if (bookStocks == null || !bookStocks.Any())
            {
                return "Error finding book, please try again.";
            }

            var book = await _context.Books.SingleAsync(x => x.Id == bookStocks.First().Book.Id);

            var availableBookStock = bookStocks
                .Where(x => x.LoanEndDate == null && x.OnLoanTo == null)
                .FirstOrDefault();

            if (availableBookStock != null)
            {
                var currentBookStock = await _context.Catalogue.Where(x => x.Id == availableBookStock.Id).SingleAsync();
                currentBookStock.LoanEndDate = DateTime.Now.AddDays(7);
                currentBookStock.OnLoanTo = borrower;

                await _context.SaveChangesAsync();
            }
            else
            {
                var reservation = new Reservation(book.Id, borrower.Id, DateTime.Now);
                await _context.Reservations.AddAsync(reservation);
                await _context.SaveChangesAsync();

                var currentReservations = await _context.Reservations
                    .Where(x => x.BookId == book.Id)
                    .CountAsync();

                resultMessage = "There are no copies of this book currently available, " +
                    "however a reservation has been made - " +
                    $"you are position {currentReservations} in the queue.";
            }

            return resultMessage;
        }
        public async Task<List<BookStock>> SearchCatalogue(CatalogueSearch search)
        {
            var list = _context.Catalogue
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
