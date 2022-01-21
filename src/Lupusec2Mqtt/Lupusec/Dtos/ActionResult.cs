using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec.Dtos
{
    public class ActionResult : JsonRespresentable
    {
        [JsonProperty("result")]
        public int Result;

        [JsonProperty("message")]
        public string Message;
    }
}
