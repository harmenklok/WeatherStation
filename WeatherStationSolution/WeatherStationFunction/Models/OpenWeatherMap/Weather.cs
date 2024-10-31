namespace WeatherStationFunction.Models.OpenWeatherMap
{

    public class Weather
    {
        public int id { get; set; }
        public string? main { get; set; }
        public required string description { get; set; }
        public string? icon { get; set; }
    }
}
