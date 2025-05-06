using CoffeePeek.Domain.Entities;
using MediatR;

namespace CoffeePeek.BusinessLogic.Models;

public abstract class BaseModel<T> where T : BaseEntity
{
    protected BaseModel(T entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }

    protected T Entity { get; }
    public int Id => Entity.Id;

    protected virtual void AddDomainEvent(INotification domainEvent)
    {
        Entity.AddDomainEvent(domainEvent);
    }

    protected virtual void AddPostDomainEvent(INotification domainEvent)
    {
        Entity.AddPostDomainEvent(domainEvent);
    }
}