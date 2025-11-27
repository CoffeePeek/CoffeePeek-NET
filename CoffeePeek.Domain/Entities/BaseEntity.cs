using MediatR;

namespace CoffeePeek.Domain.Entities;

public abstract class BaseEntity : ICreatedAt
{
    private List<INotification> _domainEvents = new();
    private List<INotification> _postDomainEvents = new();

    public virtual int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();
    public IReadOnlyCollection<INotification> PostDomainEvents => _postDomainEvents.AsReadOnly();
        
    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents.Add(eventItem);
    }
    public void AddPostDomainEvent(INotification eventItem)
    {
        _postDomainEvents.Add(eventItem);
    }

    /// <summary>
    /// Add post event only if no events of this type have been added.
    /// </summary>
    public void AddPostDomainEventIfNotExist<T>(T eventItem) where T : INotification
    {
        if (PostDomainEvents.Any(x => x is T))
        {
            return;
        }
        AddPostDomainEvent(eventItem);
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents.Remove(eventItem);
    }
    public void RemovePostDomainEvent(INotification eventItem)
    {
        _postDomainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void ClearPostDomainEvents()
    {
        _postDomainEvents.Clear();
    }
}