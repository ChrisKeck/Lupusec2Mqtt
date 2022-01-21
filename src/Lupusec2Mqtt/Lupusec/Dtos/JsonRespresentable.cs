using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec.Dtos
{
    public interface JsonRespresentable
    {
        string ToString()
        {
            return $"{JsonConvert.SerializeObject(this)}";
        }
    }

    public interface ILupusActor
    {
        int TypeIdentifier { get; }
        string CurrentStatus { get; }
    }
}