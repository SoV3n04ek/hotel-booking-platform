namespace HotelBooking.Application.DTOs.Hotels;

public record HotelSearchParameters
{
    public string? City { get; init; }
    public string? SearchTerm { get; init; }

    private int _pageNumber = 1;
    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }
    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value is > 0 and <= 50 ? value : 10;
    }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}