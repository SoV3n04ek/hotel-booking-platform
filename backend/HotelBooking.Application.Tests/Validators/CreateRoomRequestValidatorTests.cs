using FluentValidation.TestHelper;
using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.Validators;
using Xunit;

namespace HotelBooking.Application.Tests.Validators;

public class CreateRoomRequestValidatorTests
{
    private readonly CreateRoomRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Should_Have_Error_When_Capacity_Is_Out_Of_Range(int invalidCapacity)
    {
        var request = new CreateRoomRequest(1, 100m, invalidCapacity);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Zero_Or_Less()
    {
        var request = new CreateRoomRequest(1, 0m, 2);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PricePerNight);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Room_Data_Is_Valid()
    {
        var request = new CreateRoomRequest(1, 500.50m, 4);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}