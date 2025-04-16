using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApiIntegrationTests
{
    public class CatalogueRepositoryTests : IDisposable
    {
        private readonly CatalogueRepository repo;
        private readonly LibraryContext context;
        private readonly Book testBook;
        private readonly Borrower testBorrower;

        public CatalogueRepositoryTests()
        {
            repo = new();
            context = new();
            testBook = new("TestBook", new Author("TestAuthor"), BookFormat.Paperback, "1111");
            testBorrower = new("TestBorrower", "test@borrower.com");
        }

        public async void Dispose()
        {
            context.Catalogue.RemoveRange(context.Catalogue);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetLoans_ExistingBookStocksNotOnLoan_ReturnsEmptyList()
        {
            var testBookStocks = new List<BookStock>()
            {
                new(testBook),
                new(testBook)
            };
            await context.Catalogue.AddRangeAsync(testBookStocks);
            await context.SaveChangesAsync();

            var results = repo.GetLoans();

            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetLoans_ExistingBookStocksOnLoan_ReturnsAllBookStocks()
        {
            var testBookStocks = new List<BookStock>()
            {
                new(testBook)
                {
                    OnLoanTo = testBorrower,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                },
                new(testBook)
                {
                    OnLoanTo = testBorrower,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                }
            };
            await context.Catalogue.AddRangeAsync(testBookStocks);
            await context.SaveChangesAsync();

            var results = repo.GetLoans();

            Assert.NotNull(results);
            Assert.Equal(testBookStocks.Count, results.Count);
        }

        [Fact]
        public async Task GetLoans_ExistingBookStocksOnLoanAndNotOnLoan_ReturnsOnlyOnLoanBookStocks()
        {
            var testBookStocksOnLoan = new List<BookStock>()
            {
                new(testBook)
                {
                    OnLoanTo = testBorrower,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                },
                new(testBook)
                {
                    OnLoanTo = testBorrower,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                }
            };

            var testBookStocksNotOnLoan = new List<BookStock>()
            {
                new(testBook),
                new(testBook),
                new(testBook)
            };

            await context.Catalogue.AddRangeAsync(testBookStocksOnLoan);
            await context.Catalogue.AddRangeAsync(testBookStocksNotOnLoan);
            await context.SaveChangesAsync();

            var results = repo.GetLoans();

            Assert.NotNull(results);
            Assert.Equal(testBookStocksOnLoan.Count, results.Count);
        }
    }
}