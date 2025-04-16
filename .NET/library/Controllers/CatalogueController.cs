using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogueController : ControllerBase
    {
        private readonly ILogger<CatalogueController> _logger;
        private readonly ICatalogueRepository _catalogueRepository;

        public CatalogueController(ILogger<CatalogueController> logger, ICatalogueRepository catalogueRepository)
        {
            _logger = logger;
            _catalogueRepository = catalogueRepository;   
        }

        [HttpGet]
        [Route("GetCatalogue")]
        public IList<BookStock> Get()
        {
            return _catalogueRepository.GetCatalogue();
        }

        [HttpPost]
        [Route("SearchCatalogue")]
        public async Task<IList<BookStock>> Post(CatalogueSearch search)
        {
            return await _catalogueRepository.SearchCatalogue(search);
        }
    }
}