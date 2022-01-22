using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using Lupusec2Mqtt.Lupusec.Dtos;
using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec.Dtos
{
    public class RecordList
    {
        [JsonPropertyName("logrows")]
        public List<Logrow> Logrows { get; set; } = new List<Logrow>();

        public override string ToString()
        {
            return $"[\n{string.Join(",\n", Logrows.Select<ILupusActor, string>(item => $"{{\n\"name\":\"{item.Name}\",\n\"type\":\"{item.TypeId}\",\n\"status\":\"{item.Status}\"\n}}"))}\n]";
        }
    }

    public class Logrow : ILupusActor
    {
        [JsonPropertyName("uid")]
        public int Uid { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        public DateTime UtcDateTime
        {
            get
            {
                double timestamp = Convert.ToDouble(Time);
                DateTimeOffset dateTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                return dateTime.AddSeconds(timestamp).UtcDateTime;
            }
        }

        [JsonPropertyName("area")]
        public string Area { get; set; }

        [JsonPropertyName("zone")]
        public string Zone { get; set; }

        [JsonPropertyName("sid")]
        public string Sid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type_f")]
        public string TypeF { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("mark_read")]
        public int MarkRead { get; set; }
        string ILupusActor.Id
        {
            get => this.Sid;
        }
        int ILupusActor.TypeId
        {
            get
            {
                int output;
                return int.TryParse(Type, out output)?output:-1;
            }
        }
        string ILupusActor.Status
        {
            get => Event;
        }

        public override string ToString()
        {
            return $"{{\n\"name\":\"{Name}\",\n\"type\":\"{Type}\",\n\"status\":\"{Event}\"\n}}";
        }
    }


}
