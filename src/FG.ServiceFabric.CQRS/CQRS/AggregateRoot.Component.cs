namespace FG.ServiceFabric.CQRS
{
    public abstract partial class AggregateRoot<TAggregateRootEventInterface>
    {
        public abstract class Component<TAggregateRoot, TComponentEventInterface> 
            where TComponentEventInterface : class, TAggregateRootEventInterface
            where TAggregateRoot : AggregateRoot<TAggregateRootEventInterface>
        {
            private readonly DomainEventDispatcher<TComponentEventInterface> _eventDispatcher =
                new DomainEventDispatcher<TComponentEventInterface>();
            private readonly TAggregateRoot _aggregateRoot;

            protected Component(TAggregateRoot aggregateRoot) { _aggregateRoot = aggregateRoot; }

            protected virtual void RaiseEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : TComponentEventInterface
            {
                _aggregateRoot.RaiseEvent(domainEvent);
                
            }

            public void ApplyEvent(TComponentEventInterface @event)
            {
                _eventDispatcher.Dispatch(@event);
            }

            protected DomainEventDispatcher<TComponentEventInterface>.RegistrationBuilder RegisterEventAppliers()
            {
                return _eventDispatcher.RegisterHandlers();
            }
        }
    }
}