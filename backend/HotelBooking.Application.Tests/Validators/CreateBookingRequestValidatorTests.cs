using FluentValidation.TestHelper;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.Validators;
using Xunit;

namespace HotelBooking.Application.Tests.Validators;

public class CreateBookingRequestValidatorTests
{
    private readonly CreateBookingRequestValidator _validator = new();
    private readonly DateTimeOffset _futureDate = DateTimeOffset.UtcNow.AddDays(1);

    [Fact]
    public void Should_Have_Error_When_CheckIn_Is_In_Past()
    {
        // Arrange
        var request = new CreateBookingRequest(1, 1, DateTimeOffset.UtcNow.AddDays(-1), _futureDate);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CheckIn)
              .WithErrorMessage("The check-in cannot be in the past.");
    }

    [Fact]
    public void Should_Have_Error_When_CheckOut_Is_Before_CheckIn()
    {
        // Arrange
        var request = new CreateBookingRequest(1, 1, _futureDate.AddDays(2), _futureDate);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CheckOut)
              .WithErrorMessage("The check-out must be later than the check-in.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Ids_Are_Invalid(int invalidId)
    {
        var request = new CreateBookingRequest(invalidId, invalidId, _futureDate, _futureDate.AddDays(1));

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.RoomId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = new CreateBookingRequest(1, 1, _futureDate, _futureDate.AddDays(2));
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}