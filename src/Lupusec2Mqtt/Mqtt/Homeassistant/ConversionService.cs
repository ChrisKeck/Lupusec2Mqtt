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

        public IList<IDevice> GetDevices(ILupusActor sensor)
        {
            return GetDevicesFromSensor<IDevice>(sensor, new List<Logrow>());
        }

        public (ISettable Device, IDevice SwitchPowerSensor)? GetDevice(ILupusActor actor)
        {
            return GetDeviceFromPowerSwitch(actor);
        }

        public (AlarmControlPanel Area1, AlarmControlPanel Area2) GetDevice(PanelCondition panelCondition)
        {
            return (Area1: new AlarmControlPanel(_configuration, panelCondition, 1), Area2: new AlarmControlPanel(_configuration, panelCondition, 2));
        }

        public IList<IStateProvider> GetStateProviders(ILupusActor actor, IList<Logrow> logRows)
        {
            return GetDevicesFromSensor<IStateProvider>(actor, logRows);
        }

        public string GetDeviceClassDefaultValue(ILupusActor actor)
        {
            switch (actor.TypeId)
            {
                case 4: // Opener contact
                case 33: // Lock contact XT2:
                    return "window";
                case 9:
                    return "motion";
                case 11:
                    return "smoke";
                case 5:
                    return "moisture";
                default:
                    WarnUnhandledDeviceClass(actor);
                    return null;
            }
        }

        public string GetStateByStatus(ILupusActor actor, IList<Logrow> logrows)
        {
            switch (actor.TypeId)
            {
                case 4: // Opener contact
                case 33: // Lock contact XT2
                    return actor.Status == "{WEB_MSG_DC_OPEN}" ? "ON" : "OFF";
                case 9: // Motion detector
                    var matchingEvent = logrows.Where(r => r.Event.StartsWith("{ALARM_HISTORY_20}") || r.Event.StartsWith("{ALARM_HISTORY_183}"))
                                               .OrderByDescending(r => r.UtcDateTime)
                                               .FirstOrDefault(r => (DateTime.UtcNow - r.UtcDateTime) <= TimeSpan.FromSeconds(_configuration.GetValue<int>("MotionSensor:DetectionDuration")));

                    return matchingEvent != null ? "ON" : "OFF";
                case 11: // Smoke detector
                    return actor.Status == "{RPT_CID_111}" ? "ON" : "OFF";
                case 5: // Water detector
                    return "OFF";
                default:
                    WarnUnhandledState(actor);
                    return null;
            }
        }

        private IList<T> GetDevicesFromSensor<T>(ILupusActor actor, IList<Logrow> logRows) where T : IDevice
        {
            List<IStateProvider> list = new List<IStateProvider>();

            switch (actor.TypeId)
            {

                case 4: // Opener contact
                case 33: // Lock contact XT2
                case 9: // Motion detector
                case 11: // Smoke detector
                case 5: // Water detector
                    list.Add(new BinarySensor(_configuration, actor,this, logRows));
                    break;
                case 54: // Temperature/Humidity detector
                    list.Add(new TemperatureSensor(_configuration, actor, logRows));
                    list.Add(new HumiditySensor(_configuration, actor, logRows));
                    break;
                case 24:
                case 48:
                case 74:
                case 57:
                    _logger.LogDebug("This is already a device like a switch or light!");
                    break;
                case 46: // outdoor hooter
                case 45: // indoor hooter
                case 37: // keypad
                case 22: // state view
                    _logger.LogDebug(" outdoor/indoor hooter, keypad and state-viewer need to be integrated!");
                    break;
                default:
                    LogIgnoredSensor(actor);
                    break;
            }

            return list.OfType<T>().ToList();
        }

        public (IStateProvider Device, IStateProvider SwitchPowerSensor)? GetStateProvider(ILupusActor powerSwitch)
        {
            return GetDeviceFromPowerSwitch(powerSwitch);
        }

        private (ISettable Device, IStateProvider SwitchPowerSensor)? GetDeviceFromPowerSwitch(ILupusActor actor)
        {
            switch (actor.TypeId)
            {
                case 81: //New Wall switch
                case 24: // Wall switch
                    return (Device: new Switch(_configuration, actor), SwitchPowerSensor: null);
                case 48: // Power meter switch
                    return (Device: new Switch(_configuration, actor), SwitchPowerSensor: new SwitchPowerSensor(_configuration, actor));
                case 74: // Light switch
                    return (Device: new Light(_configuration, actor), SwitchPowerSensor: null);
                case 57: // Smart Lock
                    return (Device: new Lock(_configuration, actor), SwitchPowerSensor: null);
                default:
                    LogIgnoredDevice(actor);
                    return null;
            }
        }
        private void WarnUnhandledDeviceClass(ILupusActor actor)
        {
            _logger.LogWarning("This device class is not handled: {actor}", actor);
        }
        private void WarnUnhandledState(ILupusActor actor)
        {
            _logger.LogWarning("This device class is not handled: {actor}", actor);
        }
        private void LogIgnoredDevice(ILupusActor actor)
        {
            _logger.LogDebug("This device is ignored: {actor}", actor);
        }

        private void LogIgnoredSensor(ILupusActor actor)
        {
            _logger.LogDebug("This actor is ignored: {actor}", actor);
        }
    }
}
