using MediatR;

namespace Domain.Primitives
{
    // Base inmutable para eventos de dominio
    public abstract record DomainEvent : INotification
    {
        public Guid Id { get; set; }
        public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    }
}
