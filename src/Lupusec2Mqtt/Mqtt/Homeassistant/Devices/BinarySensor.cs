using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lupusec2Mqtt.Lupusec.Dtos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Lupusec2Mqtt.Mqtt.Homeassistant;
namespace Lupusec2Mqtt.Mqtt.Homeassistant.Devices
{
    public class BinarySensor : Device, IStateProvider
    {
        protected readonly Sensor _sensor;
        private readonly IConversionService _conversationService;
        protected readonly IList<Logrow> _logRows;

        [JsonProperty("device_class")]
        public string DeviceClass { get; set; }

        [JsonProperty("state_topic")]
        public string StateTopic => EscapeTopic($"homeassistant/{_component}/lupusec/{UniqueId}/state");

        [JsonIgnore]
        public string State => GetState();

        protected override string _component => "binary_sensor";

        private string GetState()
        {
            return _conversationService.GetStateByStatus(_sensor, _logRows);
        }

        public BinarySensor(IConfiguration configuration, Sensor sensor, IConversionService service, IList<Logrow> logRows = default)
            : base(configuration)
        {
            _conversationService = service;
            _sensor = sensor;
            _logRows = logRows ?? new List<Logrow>();

            UniqueId = _sensor.SensorId;
            Name = GetValue(nameof(Name), sensor.Name);
            DeviceClass = GetValue(nameof(DeviceClass), GetDeviceClassDefaultValue());
        }

        private string GetDeviceClassDefaultValue()
        {
            return _conversationService.GetDeviceClassDefaultValue(_sensor);
        }
    }
}