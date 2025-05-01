using Microsoft.EntityFrameworkCore;
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
        private int loanedDays = 7;

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
            Assert.Contains(testBorrower.Name, results.First().Borrower);
            Assert.Equal(testBorrower2.Name, results.Last().Borrower);
            Assert.Contains(testBook.Name, results.First().Books);
            Assert.Contains(testBook2.Name, results.Last().Books);
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
            Assert.Equal(testBorrower.Name, results.First().Borrower);
            Assert.Equal(testBorrower2.Name, results.Last().Borrower);
            Assert.Contains(testBook.Name, results.First().Books);
            Assert.Contains(testBook2.Name, results.Last().Books);
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

            var request = new LoanRequest(testBook2.Name, testBook2.Author.Name, testBorrower2.Name);
            var result = await repo.ReturnBook(request);

            Assert.Contains("Error", result);
            Assert.NotNull(testBookStock.OnLoanTo);
            Assert.NotNull(testBookStock.LoanEndDate);
        }

        [Fact]
        public async Task ReturnBook_ExistingBookStocksOnLoanWithinLoanDate_NullsBorrowerAndDateWithoutIncreasingFinesThenReturnsSuccessMessageWithoutFinesUpdate()
        {
            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(7)
            };
            await InsertAsync(testBookStock);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower.Name);
            var result = await repo.ReturnBook(request);

            Assert.Null(testBookStock.OnLoanTo);
            Assert.Null(testBookStock.LoanEndDate);
            Assert.Equal(0, testBorrower.FinesOwed);
            Assert.Contains("success", result);
            Assert.DoesNotContain("fine", result);
        }

        [Fact]
        public async Task ReturnBook_ExistingBookStocksOnLoanPastLoanDate_NullsBorrowerAndDateAndIncreasesFinesThenReturnsSuccessMessageWithFinesUpdate()
        {
            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(-7)
            };
            await InsertAsync(testBookStock);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower.Name);
            var result = await repo.ReturnBook(request);

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
            Assert.NotNull(await context.Reservations.SingleOrDefaultAsync(x => x.Book == testBook && x.ReservedBy == testBorrower2));
        }

        [Fact]
        public async Task LoanBook_ExistingBookStockAndExistingReservation_SetsBorrowerAndDateAndRemovesCurrentReservationThenReturnsSuccessMessage()
        {
            await InsertAsync(testBorrower);

            var testBookStock = new BookStock(testBook);
            await InsertAsync(testBookStock);

            var reservation = new Reservation(testBook.Id, testBorrower.Id, DateTime.Now.Date.AddDays(-7));
            await InsertAsync(reservation);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower.Name);
            var result = await repo.LoanBook(request);

            Assert.NotNull(testBookStock.OnLoanTo);
            Assert.NotNull(testBookStock.LoanEndDate);
            Assert.Contains("success", result);
            Assert.DoesNotContain("reservation", result);
            Assert.Empty(context.Reservations);
        }

        [Fact]
        public async Task GetAvailability_OneExistingReservationForOneBookStockNotOnLoan_ReturnsAvailableDateAsToday()
        {
            await InsertAsync(testBorrower2);

            var testBookStock = new BookStock(testBook)
            {
                OnLoanTo = null,
                LoanEndDate = null
            };
            await InsertAsync(testBookStock);

            var reservation = new Reservation(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7));
            await InsertAsync(reservation);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower2.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains("today", result);
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

        [Fact]
        public async Task GetAvailability_OneExistingReservationForTwoBookStocksOnLoanUntilToday_ReturnsAvailableDateAsToday()
        {
            await InsertAsync(testBorrower2);

            var testBookStock1 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date
            };
            var testBookStock2 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(1)
            };
            await InsertRangeAsync([testBookStock1, testBookStock2]);

            var reservation = new Reservation(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7));
            await InsertAsync(reservation);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower2.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains("today", result);
        }

        [Fact]
        public async Task GetAvailability_OneExistingReservationForTwoBookStocksOnLoanForAWeek_ReturnsAvailableDateAsEarlierLoanEndDate()
        {
            await InsertAsync(testBorrower2);

            var earlierBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays)
            };
            var laterBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 1)
            };
            await InsertRangeAsync([earlierBookStock, laterBookStock]);

            var reservation = new Reservation(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7));
            await InsertAsync(reservation);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower2.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains(earlierBookStock.LoanEndDate.Value.ToShortDateString(), result);
            Assert.Contains($"{loanedDays} days away", result);
        }

        [Fact]
        public async Task GetAvailability_MultipleExistingReservationsForTwoBookStocksRequestForLatestReservationInOddPosition_ReturnsAvailableDateAsEarlierLoanEndDatePlusMultipleWeeks()
        {
            var testBorrower3 = new Borrower("TestBorrower3", "test@borrower3.com");
            var testBorrower4 = new Borrower("TestBorrower4", "test@borrower4.com");
            var testBorrower5 = new Borrower("TestBorrower5", "test@borrower5.com");
            await InsertRangeAsync([testBorrower2, testBorrower3, testBorrower4, testBorrower5]);

            var earlierBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays)
            };
            var laterBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 1)
            };
            await InsertRangeAsync([earlierBookStock, laterBookStock]);

            var reservations = new List<Reservation>(){
                new(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7)),
                new(testBook.Id, testBorrower3.Id, DateTime.Now.Date.AddDays(-5)),
                new(testBook.Id, testBorrower4.Id, DateTime.Now.Date.AddDays(-3))
            };
            await InsertRangeAsync(reservations);

            var reservedDays = 7 * ((int)Math.Ceiling((double)reservations.Count / 2) - 1);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower4.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains(earlierBookStock.LoanEndDate.Value.AddDays(reservedDays).Date.ToShortDateString(), result);
            Assert.Contains($"{loanedDays + reservedDays} days away", result);
        }

        [Fact]
        public async Task GetAvailability_MultipleExistingReservationsForTwoBookStocksRequestForLatestReservationInEvenPosition_ReturnsAvailableDateAsLaterLoanEndDatePlusMultipleWeeks()
        {
            var testBorrower3 = new Borrower("TestBorrower3", "test@borrower3.com");
            var testBorrower4 = new Borrower("TestBorrower4", "test@borrower4.com");
            var testBorrower5 = new Borrower("TestBorrower5", "test@borrower5.com");
            await InsertRangeAsync([testBorrower2, testBorrower3, testBorrower4, testBorrower5]);

            var earlierBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays)
            };
            var laterBookStock = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 1)
            };
            await InsertRangeAsync([earlierBookStock, laterBookStock]);

            var reservations = new List<Reservation>(){
                new(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7)),
                new(testBook.Id, testBorrower3.Id, DateTime.Now.Date.AddDays(-5)),
                new(testBook.Id, testBorrower4.Id, DateTime.Now.Date.AddDays(-3)),
                new(testBook.Id, testBorrower5.Id, DateTime.Now.Date.AddDays(-1))
            };
            await InsertRangeAsync(reservations);

            var reservedDays = 7 * ((int)Math.Ceiling((double)reservations.Count / 2) - 1);

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower5.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains(laterBookStock.LoanEndDate.Value.AddDays(reservedDays).Date.ToShortDateString(), result);
            Assert.Contains($"{loanedDays + 1 + reservedDays} days away", result);
        }

        [Fact]
        public async Task GetAvailability_FiveExistingReservationsForFiveBookStocksRequestForLatestReservation_ReturnsAvailableDateAsFifthLoanEndDatePlusAWeek()
        {
            var testBorrower3 = new Borrower("TestBorrower3", "test@borrower3.com");
            var testBorrower4 = new Borrower("TestBorrower4", "test@borrower4.com");
            var testBorrower5 = new Borrower("TestBorrower5", "test@borrower5.com");
            var testBorrower6 = new Borrower("TestBorrower6", "test@borrower6.com");
            await InsertRangeAsync([testBorrower2, testBorrower3, testBorrower4, testBorrower5, testBorrower6]);

            var bookStock1 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays)
            };
            var bookStock2 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 1)
            };
            var bookStock3 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 2)
            };
            var bookStock4 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 3)
            };
            var bookStock5 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 4)
            };
            await InsertRangeAsync([bookStock1, bookStock2, bookStock3, bookStock4, bookStock5]);

            var reservations = new List<Reservation>(){
                new(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7)),
                new(testBook.Id, testBorrower3.Id, DateTime.Now.Date.AddDays(-5)),
                new(testBook.Id, testBorrower4.Id, DateTime.Now.Date.AddDays(-3)),
                new(testBook.Id, testBorrower5.Id, DateTime.Now.Date.AddDays(-1)),
                new(testBook.Id, testBorrower6.Id, DateTime.Now.Date)
            };
            await InsertRangeAsync(reservations);

            var reservedDays = 0;

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower6.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains(bookStock5.LoanEndDate.Value.AddDays(reservedDays).Date.ToShortDateString(), result);
            Assert.Contains($"{loanedDays + 4 + reservedDays} days away", result);
        }

        [Fact]
        public async Task GetAvailability_SixExistingReservationsForFiveBookStocksRequestForLatestReservation_ReturnsAvailableDateAsFirstLoanEndDatePlusMultipleWeeks()
        {
            var testBorrower3 = new Borrower("TestBorrower3", "test@borrower3.com");
            var testBorrower4 = new Borrower("TestBorrower4", "test@borrower4.com");
            var testBorrower5 = new Borrower("TestBorrower5", "test@borrower5.com");
            var testBorrower6 = new Borrower("TestBorrower6", "test@borrower6.com");
            var testBorrower7 = new Borrower("TestBorrower7", "test@borrower7.com");
            await InsertRangeAsync([testBorrower2, testBorrower3, testBorrower4, testBorrower5, testBorrower6, testBorrower7]);

            var bookStock1 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays)
            };
            var bookStock2 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 1)
            };
            var bookStock3 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 2)
            };
            var bookStock4 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 3)
            };
            var bookStock5 = new BookStock(testBook)
            {
                OnLoanTo = testBorrower,
                LoanEndDate = DateTime.Now.Date.AddDays(loanedDays + 4)
            };
            await InsertRangeAsync([bookStock1, bookStock2, bookStock3, bookStock4, bookStock5]);

            var reservations = new List<Reservation>(){
                new(testBook.Id, testBorrower2.Id, DateTime.Now.Date.AddDays(-7)),
                new(testBook.Id, testBorrower3.Id, DateTime.Now.Date.AddDays(-5)),
                new(testBook.Id, testBorrower4.Id, DateTime.Now.Date.AddDays(-3)),
                new(testBook.Id, testBorrower5.Id, DateTime.Now.Date.AddDays(-2)),
                new(testBook.Id, testBorrower6.Id, DateTime.Now.Date.AddDays(-1)),
                new(testBook.Id, testBorrower7.Id, DateTime.Now.Date)
            };
            await InsertRangeAsync(reservations);

            var reservedDays = 7;

            var request = new LoanRequest(testBook.Name, testBook.Author.Name, testBorrower7.Name);
            var result = await repo.GetAvailability(request);

            Assert.Contains(bookStock1.LoanEndDate.Value.AddDays(reservedDays).Date.ToShortDateString(), result);
            Assert.Contains($"{loanedDays + reservedDays} days away", result);
        }
    }
}