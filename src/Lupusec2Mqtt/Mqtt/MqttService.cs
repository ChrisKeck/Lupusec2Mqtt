using System;
using System.Collections.Generic;
using System.Text;
using Lupusec2Mqtt.Homeassistant;
using Microsoft.Extensions.Configuration;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Lupusec2Mqtt.Mqtt
{
    public class MqttService
    {
        private readonly MqttClient _client;

        private readonly IDictionary<string, Action<string>> _registrations = new Dictionary<string, Action<string>>();

        public MqttService(IConfiguration configuration)
        {
            _client = new MqttClient(configuration.GetMqttUrl(), configuration.GetMqttPort(), false, null, null,
                                     MqttSslProtocols.None);

            _client.MqttMsgPublishReceived += MqttMsgPublishReceived;
            _client.Connect("Lupusec2Mqtt", configuration.GetMqttUser(), configuration.GetMqttSecret());

            _client.Subscribe(new[] {"/home/temperature"}, new[] {MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE});
        }

        public void Publish(string topic, string payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            if (topic == null)
            {
                throw new ArgumentNullException("topic");
            }

            _client.Publish(topic, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
        }

        public void Register(string topic, Action<string> callback)
        {
            if (!_registrations.ContainsKey(topic))
            {
                _registrations.Add(topic, null);
            }

            _registrations[topic] = callback;

            _client.Subscribe(new[] {topic}, new[] {MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE});
        }

        private void MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (_registrations.ContainsKey(e.Topic))
            {
                _registrations[e.Topic].Invoke(Encoding.UTF8.GetString(e.Message));
            }
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }
    }
}