namespace HotelBooking.Application.DTOs.Hotels;

public record HotelSearchParameters(
    string? City = null,
    string? SearchTerm = null,
    string SortBy = "name",
    string SortOrder = "asc",
    int PageNumber = 1,
    int PageSize = 10
);