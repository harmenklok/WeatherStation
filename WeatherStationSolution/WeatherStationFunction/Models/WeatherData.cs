namespace WeatherStationFunction.Models
{

    public class WeatherData
    {
        public int TempCel { get; set; }
        public required string WindDirComp { get; set; }
        public int WindForceBft {  get; set; }
        public int UpdateInterval { get; set; }
    }
}
