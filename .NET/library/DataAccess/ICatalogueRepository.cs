using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface ICatalogueRepository
    {
        public List<BookStock> GetCatalogue();

        public Task<List<BookStock>> SearchCatalogue(CatalogueSearch search);
    }
}
