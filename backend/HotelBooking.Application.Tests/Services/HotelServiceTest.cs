using System.Linq.Expressions;
using FluentAssertions;
using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Services;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace HotelBooking.Application.Tests.Services;

public class HotelServiceTest
{
    private readonly Mock<IHotelRepository> _hotelRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly HotelService _service;

    public HotelServiceTest()
    {
        // Setup safety net for unmocked GetAll() calls
        _hotelRepo.Setup(r => r.GetAll()).Returns(new TestAsyncEnumerable<Hotel>(new List<Hotel>()));

        _service = new HotelService(_hotelRepo.Object, _uow.Object);
    }

    // ==========================================
    // Tests for CreateHotelAsync
    // ==========================================

    [Fact]
    public async Task CreateHotelAsync_ShouldSaveHotelAndReturnId()
    {
        // Arrange
        var request = new CreateHotelRequest("Grand Plaza", "123 Main St", "A nice hotel");

        // Act
        var resultId = await _service.CreateHotelAsync(request);

        // Assert
        _hotelRepo.Verify(repo => repo.AddAsync(
            It.Is<Hotel>(h =>
                h.Name == request.Name &&
                h.Address == request.Address &&
                h.Description == request.Description),
            It.IsAny<CancellationToken>()), Times.Once);

        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ==========================================
    // Tests for GetByIdAsync
    // ==========================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnHotelResponse_WhenHotelExists()
    {
        // Arrange
        var hotelId = 1;
        var hotel = new Hotel
        {
            Id = hotelId,
            Name = "Seaside Resort",
            Address = "Ocean Dr",
            Description = "Beachfront",
            Rooms = new List<Room>()
        };

        _hotelRepo.Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        // Act
        var result = await _service.GetByIdAsync(hotelId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(hotelId);
        result.Name.Should().Be("Seaside Resort");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenHotelDoesNotExist()
    {
        // Arrange
        _hotelRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ==========================================
    // Tests for SearchHotelsAsync
    // ==========================================

    [Fact]
    public async Task SearchHotelsAsync_ShouldFilterByCity()
    {
        // Arrange
        var hotels = new List<Hotel>
        {
            new() { Id = 1, Name = "Hotel A", Address = "Kyiv, Center" },
            new() { Id = 2, Name = "Hotel B", Address = "Lviv, Old Town" },
            new() { Id = 3, Name = "Hotel C", Address = "Kyiv, Suburbs" }
        };

        _hotelRepo.Setup(r => r.GetAll()).Returns(new TestAsyncEnumerable<Hotel>(hotels));

        var parameters = new HotelSearchParameters { City = "Kyiv", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.SearchHotelsAsync(parameters);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().Contain(h => h.Id == 1);
        result.Items.Should().Contain(h => h.Id == 3);
        result.Items.Should().NotContain(h => h.Id == 2);
    }

    [Fact]
    public async Task SearchHotelsAsync_ShouldFilterBySearchTerm_InNameOrDescription()
    {
        // Arrange
        var hotels = new List<Hotel>
        {
            new() { Id = 1, Name = "Luxury Resort", Description = "Basic room" },
            new() { Id = 2, Name = "Basic Motel", Description = "A luxury experience" },
            new() { Id = 3, Name = "Standard Inn", Description = "Nothing special" }
        };

        _hotelRepo.Setup(r => r.GetAll()).Returns(new TestAsyncEnumerable<Hotel>(hotels));

        var parameters = new HotelSearchParameters { SearchTerm = "Luxury", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.SearchHotelsAsync(parameters);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().Contain(h => h.Id == 1); // Found in Name
        result.Items.Should().Contain(h => h.Id == 2); // Found in Description
    }

    [Fact]
    public async Task SearchHotelsAsync_ShouldApplyDescendingSorting()
    {
        // Arrange
        var hotels = new List<Hotel>
        {
            new() { Id = 1, Name = "Alpha Hotel", Address = "A" },
            new() { Id = 2, Name = "Zeta Hotel", Address = "B" },
            new() { Id = 3, Name = "Beta Hotel", Address = "C" }
        };

        _hotelRepo.Setup(r => r.GetAll()).Returns(new TestAsyncEnumerable<Hotel>(hotels));

        var parameters = new HotelSearchParameters { SortBy = "name", SortOrder = "desc", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _service.SearchHotelsAsync(parameters);

        // Assert
        result.Items.First().Name.Should().Be("Zeta Hotel");
        result.Items.Last().Name.Should().Be("Alpha Hotel");
    }

    [Fact]
    public async Task SearchHotelsAsync_ShouldPaginateCorrectly()
    {
        // Arrange
        var hotels = Enumerable.Range(1, 25).Select(i => new Hotel
        {
            Id = i,
            Name = $"Hotel {i}",
            Address = "Address"
        }).ToList();

        _hotelRepo.Setup(r => r.GetAll()).Returns(new TestAsyncEnumerable<Hotel>(hotels));

        var parameters = new HotelSearchParameters { PageNumber = 2, PageSize = 10 };

        // Act
        var result = await _service.SearchHotelsAsync(parameters);

        // Assert
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3); // 25 / 10 = 2.5 -> Ceil -> 3
        result.Items.Should().HaveCount(10);
        result.Items.First().Id.Should().Be(11); // Page 2 should start at item 11
        result.Items.Last().Id.Should().Be(20);
    }
}