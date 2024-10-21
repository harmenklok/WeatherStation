namespace WindmeterFunction.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class WeatherData
    {
        public int windForceBft {  get; set; }
        public float tempCel {  get; set; }
        public int windDirectionDeg { get; set; }
    }
}
