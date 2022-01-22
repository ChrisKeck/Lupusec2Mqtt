﻿using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec.Dtos
{
    public class Sensor : ILupusActor
    {
        [JsonProperty("alarm_status")]
        public string AlarmStatus;

        [JsonProperty("area")]
        public byte Area;

        [JsonProperty("battery_ok")]
        public byte BatteryOk;

        [JsonProperty("battery")]
        public string BatteryText;

        [JsonProperty("bypass")]
        public string Bypassed;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("resp_mode")]
        public byte[] ResponseMode;

        [JsonProperty("sid")]
        public string SensorId;

        [JsonProperty("rssi")]
        public string SignalStrength;

        [JsonProperty("cond_ok")]
        public string StateOk;

        [JsonProperty("cond")]
        public string StateText;

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_ex")]
        public byte StatusEx;

        [JsonProperty("su")]
        public byte Su;

        [JsonProperty("tamper_ok")]
        public byte TamperOk;

        [JsonProperty("tamper")]
        public string TamperText;

        [JsonProperty("type")]
        public byte TypeId;

        [JsonProperty("type_f")]
        public string TypeName;

        [JsonProperty("zone")]
        public byte Zone;
        int ILupusActor.TypeId
        {
            get => Convert.ToInt32(this.TypeId);
        }
        string ILupusActor.Id
        {
            get => this.SensorId;
        }

        public override string ToString()
        {
            return $"{{\n\"name\":\"{Name}\",\n\"type\":{TypeId},\n\"status\":\"{Status}\"\n}}";
        }
    }
}