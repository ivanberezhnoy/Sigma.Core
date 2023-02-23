using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sigma.Core.DatabaseEntity;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class SchoolsController : ControllerBase
    {
        private readonly ILogger<CitiesController> _logger;
        private readonly DatabaseContext _dbContext;

        public SchoolsController(ILogger<CitiesController> logger, DatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet(Name = "GetAllSchools")]
        public List<School> Get()
        {
            var result = _dbContext.Schools.ToList<School>();

            return result;
        }
    }
}
