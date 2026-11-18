namespace Products.Domain;

public interface IDomainEventsDispatcher
{
    public void RaiseDomainEvent<T>(T domainEvent);
}