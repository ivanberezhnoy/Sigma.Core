using Microsoft.AspNetCore.Mvc;
using Sigma.Core.DatabaseEntity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Sigma.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CitiesController : ControllerBase
    {
        private readonly ILogger<CitiesController> _logger;
        private readonly DatabaseContext _dbContext;

        public CitiesController(ILogger<CitiesController> logger, DatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet(Name = "GetAllCities")]
        public List<City> Get()
        {
            var result = _dbContext.Cities.ToList<City>();

            return result;
        }
        [HttpPost(Name = "AddCity")]
        public string Post(City city)
        {
            var result = _dbContext.Cities.Add(city);

            try
            {
                _dbContext.SaveChanges();
            }
            catch(Exception e)
            {
                return "Failed: " + e.Message;
            }

            return "success";
        }

        [HttpPut(Name = "ChangeCity")]
        public string Put(City city)
        {
            if (city == null || city.Name == null || city.Name.Trim().Length == 0)
            {
                return "Invalid city name";
            }

            City? foundCity = _dbContext.Cities.FirstOrDefault(p => p.Id == city.Id);

            if (foundCity == null)
            {
                return "Unable found city with ID: " + city.Id;
            }

            foundCity.Name = city.Name;

            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }

            return "success";
        }

        [HttpDelete(Name = "DeleteCity")]
        public string Delete(City city)
        {
            if (city == null || city.Name == null || city.Name.Trim().Length == 0)
            {
                return "Invalid city name";
            }

            City? foundCity = _dbContext.Cities.FirstOrDefault(p => p.Id == city.Id);

            if (foundCity == null)
            {
                return "Unable found city with ID: " + city.Id;
            }

            _dbContext.Cities.Remove(foundCity);

            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }

            return "success";
        }
    }
}