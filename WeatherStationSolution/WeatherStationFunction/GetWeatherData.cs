namespace WeatherStationFunction
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Extensions.Logging;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    using WeatherStationFunction.Models;
    using WeatherStationFunction.Models.OpenWeatherMap;

    public class GetWeatherData(ILogger<GetWeatherData> logger)
    {
        private readonly ILogger<GetWeatherData> _logger = logger;

        [Function(nameof(GetWeatherData))]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            //Declare variables
            string apiKey = "26f18abcfb4cfe8441676a2681c9f638";
            string city = "Brouwershaven";
            double denominatorBft = 0.835d;
            double powerBft = (2d / 3d);
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            string[] windrichtingen = [ "Noord", "Noordoost", "Oost", "Zuidoost", "Zuid", "Zuidwest", "West", "Noordwest" ];

            using HttpClient client = new();
            try
            {
                var weatherResponse = await client.GetFromJsonAsync<WeatherResponse>(url);
                if (weatherResponse != null)
                {
                    WeatherData weatherData = new()
                    {
                        WindForceBft = Convert.ToInt16(Math.Round(Math.Pow(weatherResponse.wind.speed / denominatorBft, powerBft))),
                        TempCel = Convert.ToInt32(Math.Round(weatherResponse.main.temp, 0)),
                        WindDirectionDeg = weatherResponse.wind.deg
                    };

                    int index = (int)Math.Round(weatherData.WindDirectionDeg / 45.0) % 8;

                    Console.WriteLine($"Current temperature in {city}: {weatherData.TempCel}°C");
                    Console.WriteLine($"Weather: {weatherResponse.weather[0].description}");
                    Console.WriteLine($"The windforce is {weatherData.WindForceBft} beafort (which is {weatherResponse.wind.speed} m/s) and is heading {weatherData.WindDirectionDeg} which is {windrichtingen[index]}");
                    return new OkObjectResult(weatherData);
                }
                else
                {
                    Console.WriteLine("No weather data found.");
                    return new NotFoundObjectResult("No weather data found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new BadRequestObjectResult($"An error occurred. {ex.Message}");
            }
        }
    }
}
