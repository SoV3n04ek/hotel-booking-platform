namespace HotelBooking.Domain.Common;

public abstract class BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}

public abstract class BaseEntity<TId> : BaseEntity
{
    public TId Id { get; set; } = default!;
}