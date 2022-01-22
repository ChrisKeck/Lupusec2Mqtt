using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Lupusec2Mqtt.Mqtt.Homeassistant.Devices
{
    public abstract class Device : IDevice
    {
        private readonly IConfiguration _configuration;

        [JsonProperty("name")]
        public string Name { get; protected set; }

        [JsonProperty("unique_id")]
        public string UniqueId { get; protected set; }

        protected abstract string _component { get; }

        [JsonIgnore]
        public virtual string ConfigTopic => EscapeTopic($"homeassistant/{_component}/lupusec/{UniqueId}/config");

        protected IConfiguration Configuration
        {
            get => _configuration;
        }

        public Device(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        protected string GetValue(string property, string defaultValue)
        {
            return Configuration[$"Mappings:{UniqueId}:{property}"] ?? defaultValue;
        }

        protected string EscapeTopic(string topic)
        {
            return topic.Replace(":", "_");
        }

        public override string ToString()
        {
            return $"{JsonConvert.SerializeObject(this)}";
        }

    }
}
