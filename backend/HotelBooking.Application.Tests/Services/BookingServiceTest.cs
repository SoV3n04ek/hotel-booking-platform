using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Services;
using HotelBooking.Application.Validators;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace HotelBooking.Application.Tests.Services;

public class BookingServiceTest
{
    private readonly Mock<IBookingRepository> _bookingRepo = new();
    private readonly Mock<IRoomRepository> _roomRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IValidator<CreateBookingRequest> _validator;
    private readonly BookingService _service;
    private readonly DateTimeOffset _baseDate = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    public BookingServiceTest()
    {
        _validator = new CreateBookingRequestValidator();
        _roomRepo.Setup(r => r.GetAll()).Returns(new TestAsyncEnumerable<Room>(new List<Room>()));

        _service = new BookingService(_bookingRepo.Object, _roomRepo.Object, _uow.Object, _validator);
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowValidationException_WhenDatesAreInvalid()
    {
        var request = new CreateBookingRequest(1, 1, _baseDate.AddDays(5), _baseDate.AddDays(2));
        var failures = new List<ValidationFailure> { new("CheckOut", "The departure date must be later than the arrival date.") };

        var act = async () => await _service.CreateBookingAsync(request);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("The check-out must be later than the check-in.");
    }

    [Fact]
    public async Task CreateBooking_ShouldSucceed_WhenDataIsValid()
    {
        var room = new Room { Id = 1, PricePerNight = 100, Version = 1 };
        var request = new CreateBookingRequest(1, 1, _baseDate.AddDays(1), _baseDate.AddDays(3));

        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var result = await _service.CreateBookingAsync(request);

        result.Should().NotBeNull();
        _bookingRepo.Verify(repo => repo.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _roomRepo.Verify(repo => repo.Update(It.Is<Room>(r => r.Id == room.Id)), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowNotFound_WhenRoomDoesNotExist()
    {
        var request = new CreateBookingRequest(1, 999, _baseDate.AddDays(1), _baseDate.AddDays(2));

        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var act = async () => await _service.CreateBookingAsync(request);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Room not found.");
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowConflict_WhenRoomIsOccupied()
    {
        var request = new CreateBookingRequest(1, 1, _baseDate.AddDays(1), _baseDate.AddDays(2));

        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        _roomRepo.Setup(r => r.GetAll()).Returns(new TestAsyncEnumerable<Room>(new List<Room> { new() { Id = 1 } }));

        var act = async () => await _service.CreateBookingAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Room is already occupied for these dates.");
    }

    [Fact]
    public async Task CreateBooking_ShouldThrowConflict_WhenDbUpdateConcurrencyExceptionOccurs()
    {
        var request = new CreateBookingRequest(1, 1, _baseDate.AddDays(1), _baseDate.AddDays(2));
        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Room { Id = 1, PricePerNight = 100 });

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException());

        var act = async () => await _service.CreateBookingAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This room was just booked by someone else. Please refresh.");
    }

    [Fact]
    public async Task CreateBooking_ShouldCalculateCorrectPrice_ForMultiNightStay()
    {
        var request = new CreateBookingRequest(1, 1, _baseDate, _baseDate.AddDays(3));
        _roomRepo.Setup(r => r.GetRoomIfAvailableAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Room { Id = 1, PricePerNight = 150.50m });

        var result = await _service.CreateBookingAsync(request);

        result.TotalPrice.Should().Be(451.50m);
    }

    [Fact]
    public async Task CreateBooking_ShouldStopExecution_WhenCancellationTokenIsTriggered()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Trigger cancellation immediately

        var request = new CreateBookingRequest(1, 1, _baseDate, _baseDate.AddDays(1));

        // Act
        var act = async () => await _service.CreateBookingAsync(request, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();

        // Verify the database was never touched
        _bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBookingById_ShouldReturnResponse_WhenBookingExists()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Booking { Id = 42 });

        var result = await _service.GetBookingByIdAsync(42);

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
    }
}

// ------------------------------------------------------------------------------------------------
// INFRASTRUCTURE MOCKS FOR EF CORE 9
// ------------------------------------------------------------------------------------------------

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;
    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
    public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
    public object? Execute(Expression expression) => _inner.Execute(expression);
    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(expectedResultType).Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryable<T> _inner;
    public TestAsyncEnumerable(IEnumerable<T> enumerable) => _inner = enumerable.AsQueryable();
    public TestAsyncEnumerable(Expression expression) => _inner = new EnumerableQuery<T>(expression);
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(_inner.GetEnumerator());
    public Type ElementType => _inner.ElementType;
    public Expression Expression => _inner.Expression;
    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner.Provider);
    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _inner.GetEnumerator();
}

internal partial class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    public T Current => _inner.Current;
}