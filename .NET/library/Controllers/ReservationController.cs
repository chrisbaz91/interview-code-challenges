using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;
using System.Collections;

namespace OneBeyondApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly ILogger<ReservationController> _logger;
        private readonly ICatalogueRepository _catalogueRepository;

        public ReservationController(ILogger<ReservationController> logger, ICatalogueRepository catalogueRepository)
        {
            _logger = logger;
            _catalogueRepository = catalogueRepository;   
        }

        [HttpPost]
        [Route("LoanBook")]
        public Task<string> Post(LoanRequest request)
        {
            return _catalogueRepository.LoanBook(request);
        }
    }
}