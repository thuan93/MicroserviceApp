namespace EventBus.Messages.Common;

public abstract class IntegrationBaseEvent
{
    public Guid Id { get; private set; }
    public DateTime CreationDate { get; private set; }

    protected IntegrationBaseEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    protected IntegrationBaseEvent(Guid id, DateTime createDate)
    {
        Id = id;
        CreationDate = createDate;
    }
}
