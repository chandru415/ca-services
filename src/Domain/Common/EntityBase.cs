namespace Domain.Common
{
    public abstract record EntityBase : AuditableEntity
    {
        public Guid Id { get; private init; } = Guid.NewGuid();
    }
}
