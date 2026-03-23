using FluentValidation;
using HotelBooking.Domain.Entities;

namespace HotelBooking.Application.Validators;

public class BookingValidator : AbstractValidator<Booking>
{
    public BookingValidator()
    {
        RuleFor(x => x.DateCheckIn)
            .GreaterThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("The check-in date cannot be in the past.");

        RuleFor(x => x.DateCheckOut)
            .GreaterThanOrEqualTo(x => x.DateCheckIn)
            .WithMessage("The departure date must be later than the arrival date.");

        RuleFor(x => x.UserId).NotEmpty();
    }
}
