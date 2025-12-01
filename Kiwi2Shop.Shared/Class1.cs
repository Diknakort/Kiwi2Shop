using MassTransit;

namespace Kiwi2Shop.Shared
{

    [ExcludeFromTopology]
    public interface IRabbitEvent
    {
        public Guid EventId { get; }
        public DateTime CreatedAt { get; }
    }
}

