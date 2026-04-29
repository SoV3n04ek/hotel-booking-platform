using HotelBooking.Domain.Common;

namespace HotelBooking.Domain.Entities;

public abstract class BaseImage : BaseEntity<Guid>
{
    public string Url { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}