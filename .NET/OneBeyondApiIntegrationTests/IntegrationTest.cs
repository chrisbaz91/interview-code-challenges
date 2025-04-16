using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApiIntegrationTests
{
    public class IntegrationTest : IDisposable
    {
        protected readonly LibraryContext context;

        public IntegrationTest()
        {
            context = new();
        }

        public async void Dispose()
        {
            context.RemoveRange(context.Authors);
            context.RemoveRange(context.Books);
            context.RemoveRange(context.Borrowers);
            context.RemoveRange(context.Catalogue);
            context.RemoveRange(context.Reservations);
            await context.SaveChangesAsync();
        }

        protected async Task InsertAsync<T>(T entity) where T : Entity
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        protected async Task InsertRangeAsync<T>(List<T> entities) where T : Entity
        {
            await context.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }
    }
}