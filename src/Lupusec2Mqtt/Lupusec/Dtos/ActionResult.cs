using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec.Dtos
{
    public class ActionResult
    {
        [JsonProperty("result")]
        public int Result;

        [JsonProperty("message")]
        public string Message;

        public override string ToString()
        {
            return $"{JsonConvert.SerializeObject(this)}";
        }
    }
}
