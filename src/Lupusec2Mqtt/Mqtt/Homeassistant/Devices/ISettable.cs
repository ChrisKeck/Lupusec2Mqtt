using Lupusec2Mqtt.Lupusec;

namespace Lupusec2Mqtt.Mqtt.Homeassistant.Devices
{
    public interface ISettable : IStateProvider
    {
        string CommandTopic { get; }

        void SetState(string state, ILupusecService lupusecService);
    }
}
