using FluentValidation;
using HotelBooking.Application.DTOs.Bookings;

namespace HotelBooking.Application.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.RoomId).GreaterThan(0);

        RuleFor(x => x.CheckIn)
            .GreaterThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("The check-in cannot be in the past.");

        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn)
            .WithMessage("The check-out must be later than the check-in.");
    }
}
