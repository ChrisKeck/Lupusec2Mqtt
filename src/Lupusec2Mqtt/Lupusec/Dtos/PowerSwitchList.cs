using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec.Dtos
{
    public class PowerSwitchList
    {
        [JsonProperty("pssrows")]
        public IList<PowerSwitch> PowerSwitches { get; set; }

        public override string ToString()
        {
            return $"[\n{string.Join(",\n", PowerSwitches.Select(item => item.ToString()))}\n]";
        }
    }
}