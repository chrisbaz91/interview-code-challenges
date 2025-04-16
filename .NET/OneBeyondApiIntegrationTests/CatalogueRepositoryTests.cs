using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApiIntegrationTests
{
    public class CatalogueRepositoryTests : IntegrationTest
    {
        private readonly CatalogueRepository repo;
        private readonly Book testBook;
        private readonly Book testBook2;
        private readonly Borrower testBorrower;
        private readonly Borrower testBorrower2;

        public CatalogueRepositoryTests()
        {
            repo = new(context);
            testBook = new("TestBook", new Author("TestAuthor"), BookFormat.Paperback, "1111");
            testBook2 = new("TestBook2", new Author("TestAuthor2"), BookFormat.Hardback, "1112");
            testBorrower = new("TestBorrower", "test@borrower.com");
            testBorrower2 = new("TestBorrower2", "test@borrower2.com");
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

            var results = await repo.GetLoans();

            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetLoans_ExistingBookStocksOnLoan_ReturnsAllBorrowerLoans()
        {
            var testBookStocks = new List<BookStock>()
            {
                new(testBook)
                {
                    OnLoanTo = testBorrower,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                },
                new(testBook2)
                {
                    OnLoanTo = testBorrower2,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                }
            };
            await InsertRangeAsync(testBookStocks);

            var results = await repo.GetLoans();

            Assert.NotNull(results);
            Assert.Equal(testBookStocks.Count, results.Count());
            Assert.Contains(testBorrower.Name, results.Last().Borrower);
            Assert.Equal(testBorrower2.Name, results.First().Borrower);
            Assert.Contains(testBook.Name, results.Last().Books);
            Assert.Contains(testBook2.Name, results.First().Books);
        }

        [Fact]
        public async Task GetLoans_ExistingBookStocksOnLoanWithSameBorrower_ReturnsOneBorrowerLoan()
        {
            var testBookStocks = new List<BookStock>()
            {
                new(testBook)
                {
                    OnLoanTo = testBorrower,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                },
                new(testBook2)
                {
                    OnLoanTo = testBorrower,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                }
            };
            await InsertRangeAsync(testBookStocks);

            var results = await repo.GetLoans();

            Assert.NotNull(results);
            Assert.Single(results);
            Assert.Equal(testBorrower.Name, results.Single().Borrower);
            Assert.Contains(testBook.Name, results.Single().Books);
            Assert.Contains(testBook2.Name, results.Single().Books);
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
                new(testBook2)
                {
                    OnLoanTo = testBorrower2,
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

            var results = await repo.GetLoans();

            Assert.NotNull(results);
            Assert.Equal(testBookStocksOnLoan.Count, results.Count());
            Assert.Equal(testBorrower.Name, results.Last().Borrower);
            Assert.Equal(testBorrower2.Name, results.First().Borrower);
            Assert.Contains(testBook.Name, results.Last().Books);
            Assert.Contains(testBook2.Name, results.First().Books);
        }

        [Fact]
        public async Task ReturnBook_IncorrectReturnInfo_ReturnsErrorMessage()
        {
            var testBookStocks = new List<BookStock>()
            {
                new(testBook)
                {
                    OnLoanTo = testBorrower,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                },
                new(testBook2)
                {
                    OnLoanTo = testBorrower2,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                }
            };
            await InsertRangeAsync(testBookStocks);

            var results = await repo.ReturnBook(new Guid());

            Assert.Contains("Error", results);
        }

        [Fact]
        public async Task ReturnLoan_ExistingBookStocksOnLoan_NullsBorrowerAndDateThenReturnsSuccessMessage()
        {
            var testBookStocks = new List<BookStock>()
            {
                new(testBook)
                {
                    OnLoanTo = testBorrower,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                },
                new(testBook2)
                {
                    OnLoanTo = testBorrower2,
                    LoanEndDate = DateTime.Now.Date.AddDays(7)
                }
            };
            await InsertRangeAsync(testBookStocks);

            var result = await repo.ReturnBook(testBookStocks.First().Id);

            var updatedBookStock = context.Catalogue.First(x => x.Id == testBookStocks.First().Id);
            Assert.Null(updatedBookStock.OnLoanTo);
            Assert.Null(updatedBookStock.LoanEndDate);
            Assert.Contains("success", result);
        }
    }
}