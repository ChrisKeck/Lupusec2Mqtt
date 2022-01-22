using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lupusec2Mqtt.Lupusec.Dtos;
using Lupusec2Mqtt.Mqtt;
using Lupusec2Mqtt.Mqtt.Homeassistant;
using Lupusec2Mqtt.Mqtt.Homeassistant.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec
{
    public class PollingHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<PollingHostedService> _logger;
        private readonly ILupusecService _lupusecService;
        private readonly ConversionService _conversionService;
        private readonly IConfiguration _configuration;

        private readonly MqttService _mqttService;
        private Timer _timer;

        private int _logCounter = 0;
        private int _logEveryNCycle = 5;

        private CancellationTokenSource _cancellationTokenSource;

        public PollingHostedService(ILogger<PollingHostedService> logger, ILupusecService lupusecService, IConfiguration configuration)
        {
            _logger = logger;
            _lupusecService = lupusecService;
            _configuration = configuration;

            _conversionService = new ConversionService(_configuration, logger);
            _mqttService = new MqttService(_configuration);

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            await ConfigureSensors();
            await ConfigurePowerSwitches();
            await ConfigureAlarmPanels();

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }

        private async Task ConfigureSensors()
        {
            SensorList response = await _lupusecService.GetSensorsAsync();

            foreach (var sensor in response.Sensors)
            {
                ConfigureSensor(sensor);
            }

            AlarmBinarySensor alarmBinarySensorArea1 = new AlarmBinarySensor(_configuration, 1);
            AlarmBinarySensor alarmBinarySensorArea2 = new AlarmBinarySensor(_configuration, 2);

            PublishDeviceToMqtt(alarmBinarySensorArea1);
            PublishDeviceToMqtt(alarmBinarySensorArea2);
        }

        private void ConfigureSensor(Sensor sensor)
        {
            IEnumerable<IDevice> configs = _conversionService.GetDevices(sensor);
            foreach (var config in configs)
            { 
                PublishDeviceToMqtt(config);
            }
        }

        private async Task ConfigurePowerSwitches()
        {
            PowerSwitchList response = await _lupusecService.GetPowerSwitches();
            foreach (var powerSwitch in response.PowerSwitches)
            {
                ConfigurePowerSwitch(powerSwitch);
            }
        }

        private void ConfigurePowerSwitch(ILupusActor powerSwitch)
        {
            var config = _conversionService.GetDevice(powerSwitch);
            if (config.HasValue)
            {
                _mqttService.Register(config.Value.Device.CommandTopic, state => SetState(state, config.Value.Device));
                PublishDeviceToMqtt(config.Value.Device);
                PublishDeviceToMqtt(config.Value.SwitchPowerSensor);
            }
        }

        private void SetState(string state, ISettable device)
        {
            try
            {
                _logger.LogInformation("Switch {UniqueId} {Name} set to {Status}", device.UniqueId, device.Name, state);
                device.SetState(state, _lupusecService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while setting switch {UniqueId} {Name} to {Status}",
                                 device.UniqueId, device.Name, state);
            }
        }

        private void PublishDeviceToMqtt(IDevice device)
        {
            if (device != null)
            {
                try
                {
                    _mqttService.Publish(device.ConfigTopic, JsonConvert.SerializeObject(device));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while configure device {UniqueId} {Name}",
                                     device.UniqueId, device.Name);
                }
            }
        }

        private async Task ConfigureAlarmPanels()
        {
            PanelCondition panelCondition = await _lupusecService.GetPanelConditionAsync();
            var panelConditions = _conversionService.GetDevice(panelCondition);

            _mqttService.Register(panelConditions.Area1.CommandTopic, mode => { SetAlarm(1, mode); });
            _mqttService.Register(panelConditions.Area2.CommandTopic, mode => { SetAlarm(2, mode); });

            PublishDeviceToMqtt(panelConditions.Area1);
            PublishDeviceToMqtt(panelConditions.Area2);
        }



        private async void DoWork(object state)
        {
            try
            {
                if (--_logCounter <= 0)
                {
                    _logger.LogInformation($"Polling... (Every {_logEveryNCycle}th cycle is logged)");
                    _logCounter = _logEveryNCycle;
                }

                await PublishSensors();
                await PublishPowerSwitches();
                await PublishAlarmPanels();
            }
            catch (HttpRequestException ex)
            {
                // Log and retry on next iteration
                _logger.LogError(ex, "An error occured");
            }
            catch (Exception ex)
            {
                // Log and retry on next iteration
                _logger.LogCritical(ex, "A critical error occured");
            }
        }


        private async Task PublishPowerSwitches()
        {
            PowerSwitchList powerSwitchList = await _lupusecService.GetPowerSwitches();

            foreach (var powerSwitch in powerSwitchList.PowerSwitches)
            {
                PublishPowerSwitch(powerSwitch);
            }
        }

        private void PublishPowerSwitch(ILupusActor powerSwitch)
        {
            var device = _conversionService.GetStateProvider(powerSwitch);
            if (device.HasValue)
            {
                PublishStateToMqtt(device.Value.Device);
                PublishStateToMqtt(device.Value.SwitchPowerSensor);
            }
        }

        private void PublishStateToMqtt(IStateProvider provider)
        {
            if (provider != null)
            {
                try
                {
                    _mqttService.Publish(provider.StateTopic, provider.State);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while setting switch {UniqueId} {Name} to {Status}",
                                     provider.UniqueId, provider.Name, provider.State);
                }
            }
        }

        private async Task PublishSensors()
        {
            _logger.LogDebug("Fetching sensors from Lupusec");
            SensorList sensorList = await _lupusecService.GetSensorsAsync();
            _logger.LogDebug("Fetching records from Lupusec");
            RecordList recordList = await _lupusecService.GetRecordsAsync();

            foreach (var sensor in sensorList.Sensors)
            {
                _logger.LogDebug("Publishing sensor with name {name}.",sensor.Name);
                PublishSensor(recordList, sensor);
            }

            _logger.LogDebug("Publishing AlarmBinarySensors");
            AlarmBinarySensor alarmBinarySensorArea1 = new AlarmBinarySensor(_configuration, 1);
            AlarmBinarySensor alarmBinarySensorArea2 = new AlarmBinarySensor(_configuration, 2);

            alarmBinarySensorArea1.SetState(sensorList.Sensors);
            alarmBinarySensorArea2.SetState(sensorList.Sensors);
            PublishStateToMqtt(alarmBinarySensorArea1);
            PublishStateToMqtt(alarmBinarySensorArea2);
        }

        private void PublishSensor(RecordList recordList, ILupusActor sensor)
        {
            IList<Logrow> sensorLogRows = recordList.Logrows.Where(r => r.Sid.Equals(sensor.Id)).ToArray();
            IList<IStateProvider> devices = _conversionService.GetStateProviders(sensor, sensorLogRows);
            foreach (var device in devices)
            {
                PublishStateToMqtt(device);
            }
        }

        private async Task PublishAlarmPanels()
        {
            PanelCondition panelCondition = await _lupusecService.GetPanelConditionAsync();
            var panelConditions = _conversionService.GetDevice(panelCondition);
            _logger.LogDebug("Received alarm panel information (Area 1: {area1}, Area 2: {area2})",
                             panelConditions.Area1.State, panelConditions.Area2.State);

            PublishStateToMqtt(panelConditions.Area1);
            PublishStateToMqtt(panelConditions.Area2);
        }

        private void SetAlarm(int area, string mode)
        {
            try
            {
                _logger.LogInformation("Area {Area} set to {Mode}", area, mode);
                _lupusecService.SetAlarmMode(area, (AlarmMode)Enum.Parse(typeof(AlarmModeAction), mode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while setting alarm mode to Area {Area} set to {Mode}", area, mode);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            _mqttService.Disconnect();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}