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
            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(7)
            };
            await InsertAsync(testBookStock);

            var results = await repo.ReturnBook(new Guid());

            Assert.Contains("Error", results);
            Assert.NotNull(testBookStock.OnLoanTo);
            Assert.NotNull(testBookStock.LoanEndDate);
        }

        [Fact]
        public async Task ReturnLoan_ExistingBookStocksOnLoanWithinLoanDate_NullsBorrowerAndDateWithoutIncreasingFinesThenReturnsSuccessMessageWithoutFinesUpdate()
        {
            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(7)
            };
            await InsertAsync(testBookStock);

            var result = await repo.ReturnBook(testBookStock.Id);

            Assert.Null(testBookStock.OnLoanTo);
            Assert.Null(testBookStock.LoanEndDate);
            Assert.Equal(0, testBorrower.FinesOwed);
            Assert.Contains("success", result);
            Assert.DoesNotContain("fine", result);
        }

        [Fact]
        public async Task ReturnLoan_ExistingBookStocksOnLoanPastLoanDate_NullsBorrowerAndDateAndIncreasesFinesThenReturnsSuccessMessageWithFinesUpdate()
        {
            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(-7)
            };
            await InsertAsync(testBookStock);

            var result = await repo.ReturnBook(testBookStock.Id);

            Assert.Null(testBookStock.OnLoanTo);
            Assert.Null(testBookStock.LoanEndDate);
            Assert.Equal(3 + 0.75 * 7, testBorrower.FinesOwed);
            Assert.Contains("success", result);
            Assert.Contains("fine", result);
        }

        [Fact]
        public async Task LoanBook_ExistingBookStock_SetsBorrowerAndDateThenReturnsSuccessMessage()
        {
            await InsertAsync(testBorrower);

            var testBookStock = new BookStock(testBook);
            await InsertAsync(testBookStock);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower.Name);
            var result = await repo.LoanBook(request);

            Assert.NotNull(testBookStock.OnLoanTo);
            Assert.NotNull(testBookStock.LoanEndDate);
            Assert.Contains("success", result);
            Assert.DoesNotContain("reservation", result);
        }

        [Fact]
        public async Task LoanBook_ExistingBookStockButAlreadyOnLoan_CreatesReservationThenReturnsReservationMessage()
        {
            await InsertAsync(testBorrower2);

            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(-7)
            };
            await InsertAsync(testBookStock);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower2.Name);
            var result = await repo.LoanBook(request);

            Assert.Equal(testBorrower, testBookStock.OnLoanTo);
            Assert.Equal(DateTime.Now.Date.AddDays(-7), testBookStock.LoanEndDate);
            Assert.DoesNotContain("success", result);
            Assert.Contains("reservation", result);
            Assert.Contains("position 1", result);
        }

        [Fact]
        public async Task GetAvailability_OneExistingReservationForOneBookStockOnLoanUntilToday_ReturnsAvailableDateAsToday()
        {
            await InsertAsync(testBorrower2);

            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date
            };
            await InsertAsync(testBookStock);

            var reservation = new Reservation(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7));
            await InsertAsync(reservation);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower2.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains("today", result);
        }

        [Fact]
        public async Task GetAvailability_OneExistingReservationForOneBookStockOnLoanForAWeek_ReturnsAvailableDateAsLoanEndDate()
        {
            await InsertAsync(testBorrower2);

            var loanedDays = 7;

            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays)
            };
            await InsertAsync(testBookStock);

            var reservation = new Reservation(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7));
            await InsertAsync(reservation);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower2.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains(testBookStock.LoanEndDate.Value.ToShortDateString(), result);
            Assert.Contains($"{loanedDays} days away", result);
        }

        [Fact]
        public async Task GetAvailability_MultipleExistingReservationsForOneBookStockRequestForLatestReservation_ReturnsAvailableDateAsLoanEndDatePlusMultipleWeeks()
        {
            var testBorrower3 = new Borrower("TestBorrower3", "test@borrower3.com");
            var testBorrower4 = new Borrower("TestBorrower4", "test@borrower4.com");
            var testBorrower5 = new Borrower("TestBorrower5", "test@borrower5.com");
            await InsertRangeAsync([testBorrower2, testBorrower3, testBorrower4, testBorrower5]);

            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date
            };
            await InsertAsync(testBookStock);

            var reservations = new List<Reservation>(){
                new(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7)),
                new(testBook.Id, testBorrower3.Id, DateTime.Now.Date.AddDays(-5)),
                new(testBook.Id, testBorrower4.Id, DateTime.Now.Date.AddDays(-3)),
                new(testBook.Id, testBorrower5.Id, DateTime.Now.Date.AddDays(-1))
            };
            await InsertRangeAsync(reservations);

            var reservedDays = (reservations.Count - 1) * 7;

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower5.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains(testBookStock.LoanEndDate.Value.AddDays(reservedDays).Date.ToShortDateString(), result);
            Assert.Contains($"{reservedDays} days away", result);
        }
    }
}