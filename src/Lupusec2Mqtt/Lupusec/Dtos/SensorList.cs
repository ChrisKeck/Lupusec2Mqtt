using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec.Dtos
{
    public class SensorList
    {
        [JsonProperty("senrows")]
        public IList<Sensor> Sensors;

        public override string ToString()
        {
            return $"[\n{string.Join(",\n", Sensors.Select(item => item.ToString()))}\n]";
        }
    }
}