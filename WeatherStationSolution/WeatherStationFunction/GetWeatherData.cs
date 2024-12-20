namespace WeatherStationFunction
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Extensions.Logging;

    using System.Configuration;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;

    using WeatherStationFunction.Models;
    using WeatherStationFunction.Models.OpenWeatherMap;

    public class GetWeatherData(ILogger<GetWeatherData> logger)
    {

        [Function(nameof(GetWeatherData))]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {

            string apiKey = ReadConfigValue("OpenWeatherMapToken");
            string city = ReadConfigValue("City");
            int updateInterval = string.IsNullOrEmpty(ReadConfigValue("UpdateInterval", false)) ? 5 : Convert.ToInt16(ReadConfigValue("UpdateInterval"));

            //Constants for speed to Bft calculation
            double denominatorBft = 0.8353d;
            double powerBft = (2d / 3d);
            
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            string[] windCompassDirs = [ "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "Northwest" ];
            string[] windCompDirs = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];

            using HttpClient client = new();
            try
            {
                var weatherResponse = await client.GetFromJsonAsync<WeatherResponse>(url);
                if (weatherResponse != null)
                {
                    double windForce = Math.Pow(weatherResponse.wind.speed / denominatorBft, powerBft); //Machische formule om windkracht te berekenen
                    windForce = Math.Round(windForce); //Afronden op geheel getal
                    windForce = Math.Min(windForce, 12); //Maximale windkracht is 12

                    int compIdx = (int)Math.Round(weatherResponse.wind.deg / 45.0) % 8; //Berekenen van de windrichting

                    WeatherData weatherData = new()
                    {
                        WindForceBft = Convert.ToInt16(windForce),
                        TempCel = Convert.ToInt16(Math.Round(weatherResponse.main.temp, 0)),
                        WindDirComp = windCompDirs[compIdx],
                        UpdateInterval = updateInterval
                    };

                    logger.LogInformation($"Temperature in {city} is {weatherData.TempCel}°C");
                    logger.LogInformation($"Wind is {weatherResponse.wind.speed} m/s which is {weatherData.WindForceBft} beaufort and is heading {windCompassDirs[compIdx]} ({weatherResponse.wind.deg}°)");
                    logger.LogInformation($"Response is: {JsonSerializer.Serialize(weatherData)}");
                    return new OkObjectResult(weatherData);
                }
                else
                {
                    logger.LogWarning("No weather data found.");
                    return new NotFoundObjectResult("No weather data found.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
                return new BadRequestObjectResult($"An error occurred. {ex.Message}");
            }
        }

        public static string ReadConfigValue(string name, bool throwError = true)
        {
            string? result = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(result))
            {
                if (throwError)
                {
                    throw new ConfigurationErrorsException("Config value '" + name + "' is not set!");
                }
                else
                {
                    result = string.Empty;
                }
            }

            return result;
        }
    }
}
