using System;
using System.Collections.Generic;
using System.Linq;
using Lupusec2Mqtt.Lupusec.Dtos;
using Lupusec2Mqtt.Mqtt.Homeassistant.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lupusec2Mqtt.Mqtt.Homeassistant
{
    public class ConversionService : IConversionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public ConversionService(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IList<IDevice> GetDevices(Sensor sensor)
        {
            return GetDevicesFromSensor<IDevice>(sensor, new List<Logrow>());
        }

        public (ISettable Device, IDevice SwitchPowerSensor)? GetDevice(PowerSwitch powerSwitch)
        {
            return GetDeviceFromPowerSwitch(powerSwitch);
        }

        public (AlarmControlPanel Area1, AlarmControlPanel Area2) GetDevice(PanelCondition panelCondition)
        {
            return (Area1: new AlarmControlPanel(_configuration, panelCondition, 1), Area2: new AlarmControlPanel(_configuration, panelCondition, 2));
        }

        public IList<IStateProvider> GetStateProviders(Sensor sensor, IList<Logrow> logRows)
        {
            return GetDevicesFromSensor<IStateProvider>(sensor, logRows);
        }

        public string GetDeviceClassDefaultValue(ILupusActor sensor)
        {
            switch (sensor.TypeIdentifier)
            {
                case 4: // Opener contact
                case 33: // Opener contact XT2:
                    return "window";
                case 9:
                    return "motion";
                case 11:
                    return "smoke";
                case 5:
                    return "moisture";
                default:
                    return null;
            }
        }

        public string GetStateByStatus(ILupusActor sensor, IList<Logrow> logrows)
        {
            switch (sensor.TypeIdentifier)
            {
                case 4: // Opener contact
                case 33: // Lock contact XT2
                    return sensor.CurrentStatus == "{WEB_MSG_DC_OPEN}" ? "ON" : "OFF";
                case 9: // Motion detector
                    var matchingEvent = logrows.Where(r => r.Event.StartsWith("{ALARM_HISTORY_20}") || r.Event.StartsWith("{ALARM_HISTORY_183}"))
                                               .OrderByDescending(r => r.UtcDateTime)
                                               .FirstOrDefault(r => (DateTime.UtcNow - r.UtcDateTime) <= TimeSpan.FromSeconds(_configuration.GetValue<int>("MotionSensor:DetectionDuration")));

                    return matchingEvent != null ? "ON" : "OFF";
                case 11: // Smoke detector
                    return sensor.CurrentStatus == "{RPT_CID_111}" ? "ON" : "OFF";
                case 5: // Water detector
                    return "OFF";
                default:
                    return null;
            }
        }

        public IList<T> GetDevicesFromSensor<T>(Sensor sensor, IList<Logrow> logRows) where T : IDevice
        {
            List<IStateProvider> list = new List<IStateProvider>();

            switch (sensor.TypeId)
            {
                case 24:
                case 48:
                case 74:
                case 57:
                    _logger.LogDebug("This is already a device like a switch or light!");
                    break;
                case 4: // Opener contact
                case 33: // Lock contact XT2
                case 9: // Motion detector
                case 11: // Smoke detector
                case 5: // Water detector
                    list.Add(new BinarySensor(_configuration, sensor,this, logRows));
                    break;
                case 54: // Temperature/Humidity detector
                    list.Add(new TemperatureSensor(_configuration, sensor, logRows));
                    list.Add(new HumiditySensor(_configuration, sensor, logRows));
                    break;
                default:
                    LogIgnoredSensor(sensor);
                    break;
            }

            return list.OfType<T>().ToList();
        }

        public (IStateProvider Device, IStateProvider SwitchPowerSensor)? GetStateProvider(PowerSwitch powerSwitch)
        {
            return GetDeviceFromPowerSwitch(powerSwitch);
        }

        public (ISettable Device, IStateProvider SwitchPowerSensor)? GetDeviceFromPowerSwitch(PowerSwitch powerSwitch)
        {
            switch (powerSwitch.Type)
            {
                case 24: // Wall switch
                    return (Device: new Switch(_configuration, powerSwitch), SwitchPowerSensor: null);
                case 48: // Power meter switch
                    return (Device: new Switch(_configuration, powerSwitch), SwitchPowerSensor: new SwitchPowerSensor(_configuration, powerSwitch));
                case 74: // Light switch
                    return (Device: new Light(_configuration, powerSwitch), SwitchPowerSensor: null);
                case 57: // Smart Lock
                    return (Device: new Lock(_configuration, powerSwitch), SwitchPowerSensor: null);
                default:
                    LogIgnoredDevice(powerSwitch);
                    return null;
            }
        }

        private void LogIgnoredDevice(PowerSwitch powerSwitch)
        {
            _logger.LogDebug("This device is ignored: {powerSwitch}", powerSwitch);
        }

        private void LogIgnoredSensor(Sensor sensor)
        {
            _logger.LogDebug("This sensor is ignored: {sensor}", sensor);
        }
    }
}
