using Newtonsoft.Json;

namespace Lupusec2Mqtt.Lupusec.Dtos
{
    public interface ILupusActor
    {
        string Name { get; }
        string Id { get; }
        int TypeId { get; }
        string Status { get; }
    }
}