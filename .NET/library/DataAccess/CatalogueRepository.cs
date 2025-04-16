using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class CatalogueRepository : ICatalogueRepository
    {
        public CatalogueRepository()
        {
        }

        public List<BookStock> GetCatalogue()
        {
            using var context = new LibraryContext();
            var list = context.Catalogue
                .Include(x => x.Book)
                .ThenInclude(x => x.Author)
                .Include(x => x.OnLoanTo)
                .ToList();
            return list;
        }

        public async Task<IEnumerable<BorrowerLoans>> GetLoans()
        {
            using var context = new LibraryContext();

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

        public async Task<string> ReturnBook(Guid guid)
        {
            using var context = new LibraryContext();

            var bookStock = await context.Catalogue
                .Include(x => x.OnLoanTo)
                .SingleOrDefaultAsync(x => x.Id == guid);

            if (bookStock != null
                && bookStock.LoanEndDate != null
                && bookStock.OnLoanTo != null)
            {
                bookStock.LoanEndDate = null;
                bookStock.OnLoanTo = null;
                await context.SaveChangesAsync();
                return "Book successfully returned, thank you!";
            }

            return "Error finding loan, please try again.";
        }
        public async Task<List<BookStock>> SearchCatalogue(CatalogueSearch search)
        {
            using var context = new LibraryContext();
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
