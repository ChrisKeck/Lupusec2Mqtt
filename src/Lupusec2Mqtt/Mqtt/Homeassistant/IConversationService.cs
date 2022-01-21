using System.Collections.Generic;
using Lupusec2Mqtt.Lupusec.Dtos;

namespace Lupusec2Mqtt.Mqtt.Homeassistant
{
    public interface IConversionService
    {
        string GetStateByStatus(ILupusActor sensor, IList<Logrow> logrows);
        string GetDeviceClassDefaultValue(ILupusActor device);
    }
}