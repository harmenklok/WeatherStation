namespace WeatherStationFunction.Models.OpenWeatherMap
{
    using System.Collections.Generic;

    public class WeatherResponse
    {
        public required List<Weather> weather { get; set; }
        public string? WindDirComp { get; set; }
        public required Main main { get; set; }
        public int visibility { get; set; }
        public required Wind wind { get; set; }        
        public int dt { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string? name { get; set; }
        public int cod { get; set; }
    }
}
