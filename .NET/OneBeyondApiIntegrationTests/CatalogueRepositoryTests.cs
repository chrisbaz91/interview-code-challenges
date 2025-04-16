using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApiIntegrationTests
{
    public class CatalogueRepositoryTests : IntegrationTest
    {
        private readonly CatalogueRepository repo;
        private readonly Book testBook;
        private readonly Borrower testBorrower;

        public CatalogueRepositoryTests()
        {
            repo = new();
            testBook = new("TestBook", new Author("TestAuthor"), BookFormat.Paperback, "1111");
            testBorrower = new("TestBorrower", "test@borrower.com");
        }

        [Fact]
        public async Task GetLoans_ExistingBookStocksNotOnLoan_ReturnsEmptyList()
        {
            var testBookStocks = new List<BookStock>()
            {
                new(testBook),
                new(testBook)
            };
            await InsertRangeAsync(testBookStocks);

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
            await InsertRangeAsync(testBookStocks);

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

            await InsertRangeAsync(testBookStocksOnLoan);
            await InsertRangeAsync(testBookStocksNotOnLoan);

            var results = repo.GetLoans();

            Assert.NotNull(results);
            Assert.Equal(testBookStocksOnLoan.Count, results.Count);
        }
    }
}