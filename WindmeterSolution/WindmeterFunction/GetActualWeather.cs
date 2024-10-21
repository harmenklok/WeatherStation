namespace WindmeterFunction
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Extensions.Logging;

    using WindmeterFunction.Models;

    public class GetActualWeather(ILogger<GetActualWeather> logger)
    {
        private readonly ILogger<GetActualWeather> _logger = logger;

        [Function(nameof(GetActualWeather))]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            WeatherData weatherData = new()
            {
                windForceBft = 7,
                tempCel = 16.34F,
                windDirectionDeg = 234
            };

            return new OkObjectResult(weatherData);
        }
    }
}
