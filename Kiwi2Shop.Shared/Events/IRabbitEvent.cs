using MassTransit;

namespace Kiwi2Shop.Shared.Events;


[ExcludeFromTopology]
public interface IRabbitEvent
{
    public Guid EventId { get; }
    public DateTime CreatedAt { get; }
}
