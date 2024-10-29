namespace WeatherStationFunction
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using System.Configuration;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    using WeatherStationFunction.Models;
    using WeatherStationFunction.Models.OpenWeatherMap;

    public class GetWeatherData(ILogger<GetWeatherData> logger)
    {

        [Function(nameof(GetWeatherData))]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            string apiKey = ReadConfigValue("OpenWeatherMapToken");
            string city = ReadConfigValue("City");
            double denominatorBft = 0.8353d;
            double powerBft = (2d / 3d);
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            string[] windrichtingen = [ "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "Northwest" ];

            using HttpClient client = new();
            try
            {
                var weatherResponse = await client.GetFromJsonAsync<WeatherResponse>(url);
                if (weatherResponse != null)
                {
                    double windForce = Math.Pow(weatherResponse.wind.speed / denominatorBft, powerBft); //Machische formule om windkracht te berekenen
                    windForce = Math.Round(windForce); //Afronden op geheel getal
                    windForce = Math.Min(windForce, 12); //Maximale windkracht is 12

                    WeatherData weatherData = new()
                    {
                        WindForceBft = Convert.ToInt16(windForce),
                        TempCel = Convert.ToInt16(Math.Round(weatherResponse.main.temp, 0)),
                        WindDirectionDeg = weatherResponse.wind.deg
                    };

                    int index = (int)Math.Round(weatherData.WindDirectionDeg / 45.0) % 8; //Berekenen van de windrichting

                    logger.LogInformation($"Current temperature in {city}: {weatherData.TempCel}�C");
                    logger.LogInformation($"Weather: {weatherResponse.weather[0].description}");
                    logger.LogInformation($"The windforce is {weatherData.WindForceBft} beaufort (which is {weatherResponse.wind.speed} m/s) and is heading {weatherData.WindDirectionDeg} which is {windrichtingen[index]}");
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
            string? environmentVariable = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(environmentVariable) && throwError)
            {
                throw new ConfigurationErrorsException("Config value '" + name + "' is not set!");
            }

            return environmentVariable;
        }
    }
}
