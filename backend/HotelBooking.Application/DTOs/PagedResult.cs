namespace HotelBooking.Application.DTOs;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);