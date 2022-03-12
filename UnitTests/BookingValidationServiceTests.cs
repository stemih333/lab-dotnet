using AppLogic.Services;
using Common.Exceptions;
using Common.Interfaces;
using Models.Entities;
using Models.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class BookingValidationServiceTests
    {
        private readonly string[] _validAccounts = new[] { "1000", "1010", "2000", "2210" };
        [Fact]
        public async Task ValidateBooking_Pass()
        {
            // Arrange
            var testDate = new DateTime(2021, 12, 1);
            
            var booking = GetBaseBooking(testDate);
            booking.Rows = new List<BookingRow>
            {
                GetBaseBookingRow("1010", 100, "123ABC", "1234"),
                GetBaseBookingRow("2210", -100),
            };

            var validator = GetValidator(testDate);

            // Act
            await validator.ValidateBookingAsync(booking, _validAccounts);

            // No asserts
        }

        [Fact]
        public async Task ValidateBooking_InvalidData()
        {
            // Arrange
            var testDate = new DateTime(2021, 12, 1);

            var booking = GetBaseBooking(testDate);
            booking.Rows = new List<BookingRow>
            {
                GetBaseBookingRow("1010123", 100, "123 ABC", "ABC"),
                GetBaseBookingRow("2210", -100),
            };

            var validator = GetValidator(testDate);

            // Act
            var exception = await Assert.ThrowsAsync<AppValidationException>(() => validator.ValidateBookingAsync(booking, _validAccounts));

            // Assert
            Assert.NotEmpty(exception.Errors);
            Assert.Equal(3, exception.Errors?.Count());
        }

        [Theory]
        [InlineData("2021-12-01", "2021-11-30", "A previous period cannot be used as booking date.")]
        [InlineData("2021-12-01", "2021-10-30", "A previous period cannot be used as booking date.")]
        [InlineData("2021-12-01", "2022-02-01", "Bookings cannot have period that is more than two months in future.")]
        [InlineData("2021-12-01", "2022-05-01", "Bookings cannot have period that is more than two months in future.")]
        public async Task ValidateBooking_BookingDates(string testDateString, string bookingDateString, string errorMessage)
        {
            // Arrange
            var testDate = DateTime.Parse(testDateString);
            var bookingDate = DateTime.Parse(bookingDateString);

            var booking = GetBaseBooking(bookingDate);
            booking.Rows = new List<BookingRow>
            {
                GetBaseBookingRow("1010", 100, "123ABC", "1234"),
                GetBaseBookingRow("2210", -100),
            };

            var validator = GetValidator(testDate);

            // Act
            var exception = await Assert.ThrowsAsync<AppValidationException>(() => validator.ValidateBookingAsync(booking, _validAccounts));

            // Assert
            Assert.NotEmpty(exception.Errors);
            Assert.Equal("BookingDate", exception.Errors?.First().Property);
            Assert.Equal(errorMessage, exception.Errors?.First().ErrorMessage);
        }

        [Fact]
        public async Task ValidateBooking_UnbalancedAmount()
        {
            // Arrange
            var testDate = new DateTime(2021, 12, 1);

            var booking = GetBaseBooking(testDate);
            booking.Rows = new List<BookingRow>
            {
                GetBaseBookingRow("1010", 100, "123ABC", "1234"),
                GetBaseBookingRow("2210", new decimal(-100.10)),
            };

            var validator = GetValidator(testDate);

            // Act
            var exception = await Assert.ThrowsAsync<AppValidationException>(() => validator.ValidateBookingAsync(booking, _validAccounts));

            // Assert
            Assert.NotEmpty(exception.Errors);
            Assert.Equal("Amount", exception.Errors?.First().Property);
            Assert.Equal("All the booking rows do not balance.", exception.Errors?.First().ErrorMessage);
        }

        private static BookingValidationService GetValidator(DateTime testDate)
        {
            var dateTimeService = new Mock<IDateTimeService>();
            dateTimeService.Setup(_ => _.GetUtcDateTime()).Returns(testDate);

            return new BookingValidationService(dateTimeService.Object);
        }

        private static Booking GetBaseBooking(DateTime? bookingDate = default)
        {
            var testDate = new DateTime(2021, 12, 1);
            var testUser = "stefan.mihailovic@if.se";
            return new Booking()
            {
                BookingDate = bookingDate,
                BookingStatus = BookingStatus.Saved,
                Id = 1,
                Created = testDate,
                Updated = testDate,
                UpdatedBy = testUser,
                CreatedBy = testUser
            };
        }

        private static BookingRow GetBaseBookingRow(string? account = default, decimal? amount = default, string? costCenter = default, string? subAccount = default)
        {
            var testDate = new DateTime(2021, 12, 1);
            var testUser = "stefan.mihailovic@if.se";
            return new BookingRow { 
                BookingId = 1,
                Account = account,
                Amount = amount,
                CostCenter = costCenter,
                SubAccount = subAccount,
                Created = testDate, 
                Updated = testDate, 
                UpdatedBy = testUser, 
                CreatedBy = testUser, 
            };
        }
    }
}