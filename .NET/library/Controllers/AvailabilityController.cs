using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AvailabilityController(ILogger<AvailabilityController> logger, ICatalogueRepository catalogueRepository) : ControllerBase
    {
        private readonly ILogger<AvailabilityController> _logger = logger;
        private readonly ICatalogueRepository _catalogueRepository = catalogueRepository;

        [HttpPost]
        [Route("GetAvailability")]
        public Task<string> Post(LoanRequest request)
        {
            return _catalogueRepository.GetAvailability(request);
        }
    }
}