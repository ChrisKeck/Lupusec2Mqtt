using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lupusec2Mqtt.Lupusec.Dtos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Lupusec2Mqtt.Mqtt.Homeassistant.Devices
{
    public class HumiditySensor: Device, IStateProvider
    {
        protected readonly ILupusActor _sensor;
        protected readonly IList<Logrow> _logRows;

        [JsonProperty("device_class")]
        public string DeviceClass { get; set; }

        [JsonProperty("state_topic")]
        public string StateTopic => EscapeTopic($"homeassistant/{_component}/lupusec/{UniqueId}/state");

        [JsonIgnore]
        public string State => GetState();

        [JsonProperty("unit_of_measurement")]
        public string UnitOfMeasurement => "%";

        protected override string _component => "sensor";

        private string GetState()
        {
            var match = Regex.Match(_sensor.Status, @"{WEB_MSG_RH_HUMIDITY}\s*(?'value'\d+\.?\d*)");

            if (match.Success) { return match.Groups["value"].Value; }
            return "0";
        }

        public HumiditySensor(IConfiguration configuration, ILupusActor sensor, IList<Logrow> logRows = default)
        : base(configuration)
        {
            _sensor = sensor;
            _logRows = logRows??new List<Logrow>();

            UniqueId = _sensor.Id + "HUMIDITY";
            Name = GetValue(nameof(Name), sensor.Name + " - Humidity");
            DeviceClass = GetValue(nameof(DeviceClass), GetDeviceClassDefaultValue());
        }

        private string GetDeviceClassDefaultValue()
        {
            return "humidity";
        }
    }
}
