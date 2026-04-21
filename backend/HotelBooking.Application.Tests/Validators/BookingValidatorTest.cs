using FluentValidation.TestHelper;
using HotelBooking.Application.Validators;
using HotelBooking.Domain.Entities;
using Xunit;

namespace HotelBooking.Application.Tests.Validators;

public class BookingValidatorTest
{
    private readonly BookingValidator _validator;

    public BookingValidatorTest()
    {
        _validator = new BookingValidator();
    }

    [Fact]
    public void Should_Have_Error_When_CheckIn_Is_In_Past()
    {
        // Arrange
        var booking = new Booking
        {
            DateCheckIn = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Act
        // TestValidate is an extension method from FluentValidation.TestHelper
        var result = _validator.TestValidate(booking);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateCheckIn)
            .WithErrorMessage("The check-in date cannot be in the past.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Dates_Are_Valid()
    {
        // Arrange
        var booking = new Booking
        {
            DateCheckIn = DateTimeOffset.UtcNow.AddDays(1),
            DateCheckOut = DateTimeOffset.UtcNow.AddDays(3),
            UserId = 1,
            RoomId = 1
        };

        // Act
        var result = _validator.TestValidate(booking);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}