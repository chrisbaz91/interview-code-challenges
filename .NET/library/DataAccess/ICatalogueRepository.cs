using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface ICatalogueRepository
    {
        public List<BookStock> GetCatalogue();

        public Task<List<BookStock>> SearchCatalogue(CatalogueSearch search);

        public Task<IEnumerable<BorrowerLoans>> GetLoans();

        public Task<string> ReturnBook(LoanRequest request);

        public Task<string> LoanBook(LoanRequest request);

        public Task<string> GetAvailability(LoanRequest request);
    }
}
