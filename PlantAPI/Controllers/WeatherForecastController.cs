using Microsoft.AspNetCore.Mvc;

namespace PlantAPI.Controllers
{
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

       

        //[HttpPost, Route("uploadFromCamera")]
        //public bool Post(object obj)
        //{
        //    return true;
        //}
    }
}
