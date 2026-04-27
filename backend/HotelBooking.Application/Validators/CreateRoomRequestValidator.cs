using HotelBooking.Application.DTOs.Rooms;
using FluentValidation;

namespace HotelBooking.Application.Validators;

public class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator() 
    {
        RuleFor(x => x.PricePerNight)
            .GreaterThan(0)
            .WithMessage("Price per night must be greater than zero.");

        RuleFor(x => x.Capacity)
            .InclusiveBetween(1, 10)
            .WithMessage("Capacity must be between 1 and 10 guests.");

        RuleFor(x => x.HotelId)
            .GreaterThan(0)
            .WithMessage("A valid HotelId is required.");
    }
}

