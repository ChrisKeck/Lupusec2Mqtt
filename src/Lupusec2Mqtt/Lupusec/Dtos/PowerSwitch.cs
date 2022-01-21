using System;
using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec.Dtos
{
    public class PowerSwitch : JsonRespresentable, ILupusActor
    {
        [JsonProperty("area")]
        public int Area { get; set; }

        [JsonProperty("zone")]
        public int Zone { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("type_f")]
        public string TypeF { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("consumer_id")]
        public int ConsumerId { get; set; }

        [JsonProperty("ammeter")]
        public int Ammeter { get; set; }

        [JsonProperty("always_off")]
        public int AlwaysOff { get; set; }

        [JsonProperty("shutter_turn")]
        public int ShutterTurn { get; set; }

        [JsonProperty("hue")]
        public string Hue { get; set; }

        [JsonProperty("sat")]
        public string Sat { get; set; }

        [JsonProperty("ctemp")]
        public string Ctemp { get; set; }

        [JsonProperty("hue_cmode")]
        public string HueCmode { get; set; }

        [JsonProperty("hue_cie_x")]
        public string HueCieX { get; set; }

        [JsonProperty("hue_cie_y")]
        public string HueCieY { get; set; }

        [JsonProperty("hue_color_cap")]
        public string HueColorCap { get; set; }

        [JsonProperty("nuki")]
        public string Nuki { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        int ILupusActor.TypeIdentifier => Type;
        string ILupusActor.CurrentStatus => Status;
    }
}