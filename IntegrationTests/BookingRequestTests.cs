using AppLogic.Bookings;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Models.Entities;
using Respawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace IntegrationTests;

public class BookingRequestTests
{
    [Fact]
    public async void NewBookingHandler_Pass()
    {
        // Arrange
        var auth = new TestAuthService();
        var dateTime = Utils.GetDateTimeService(DateTime.UtcNow);
        var context = Utils.GetDbContext(auth, dateTime);
        var mapper = Utils.GetAutoMapper();
        var command = new NewBookingCommand
        {
            Approver = "Test",
            BookingDate = DateTime.Now,
            Rows = new List<BookingRowDto>
            {
                new BookingRowDto { Account = "1010", Amount = 100, CostCenter = "123AB", SubAccount = "12345" },
                new BookingRowDto { Account = "2010", Amount = -100 }
            }
        };
        var handler = new NewBookingCommand.NewBookingHandler(mapper, context, auth);
        var validator = new NewBookingCommand.NewBookingValidation();

        // Act
        var validateRes = await validator.ValidateAsync(command);
        var res = await handler.Handle(command, new CancellationToken());

        // Assert
        Assert.True(validateRes.IsValid);
        Assert.NotEqual(0, res.Id);
        Assert.True(context.Bookings.Count() == 1);
        Assert.True(context.BookingRows.Count() == 2);

        await _checkpoint.Reset(Utils.ConnectionString);
    }

    [Fact]
    public async void UpdateBookingHandler_Pass()
    {
        // Arrange
        var auth = new TestAuthService();
        var dateTime = Utils.GetDateTimeService(new DateTime(2021,12,1));
        var context = Utils.GetDbContext(auth, dateTime);
        var testBooking = GetTestBooking(dateTime.GetUtcDateTime());
        await context.Bookings.AddAsync(testBooking);
        await context.SaveChangesAsync();

        var newBookingDate = new DateTime(2021, 12, 2);
        var command = new UpdateBookingCommand
        {
            BookingId = testBooking.Id,
            Approver = "Test",
            BookingDate = newBookingDate,
            Rows = new List<BookingRowDto>
            {
                new BookingRowDto { Id = testBooking.Rows.First().Id,  Account = "3010", Amount = 200 },
                new BookingRowDto { Id = testBooking.Rows.Last().Id, Account = "4010", Amount = -200 }
            }
        };
        var handler = new UpdateBookingCommand.UpdateBookingHandler(context, auth);
        var validator = new UpdateBookingCommand.UpdateBookingValidation();

        // Act
        var validateRes = await validator.ValidateAsync(command);
        await handler.Handle(command, new CancellationToken());
        var updatedBooking = context.Bookings.Include(_ => _.Rows).FirstOrDefault(_ => _.Id == testBooking.Id);

        // Assert
        Assert.True(validateRes.IsValid);
        Assert.NotNull(updatedBooking);
        Assert.Equal(command.BookingId, updatedBooking?.Id);
        Assert.Equal(command.BookingDate, updatedBooking?.BookingDate!.Value.Date);
        Assert.Equal(command.Approver, updatedBooking?.Approver);
        Assert.Equal(command.Rows.Count, updatedBooking?.Rows.Count());
        Assert.Equal(command.Rows.FirstOrDefault()?.Id, updatedBooking?.Rows.FirstOrDefault()?.Id);
        Assert.Equal(command.Rows.FirstOrDefault()?.Account, updatedBooking?.Rows.FirstOrDefault()?.Account);
        Assert.Equal(command.Rows.FirstOrDefault()?.SubAccount, updatedBooking?.Rows.FirstOrDefault()?.SubAccount);
        Assert.Equal(command.Rows.FirstOrDefault()?.Amount, updatedBooking?.Rows.FirstOrDefault()?.Amount);
        Assert.Equal(command.Rows.FirstOrDefault()?.CostCenter, updatedBooking?.Rows.FirstOrDefault()?.CostCenter);
        Assert.Equal(command.Rows.LastOrDefault()?.Id, updatedBooking?.Rows.LastOrDefault()?.Id);
        Assert.Equal(command.Rows.LastOrDefault()?.Account, updatedBooking?.Rows.LastOrDefault()?.Account);
        Assert.Equal(command.Rows.LastOrDefault()?.SubAccount, updatedBooking?.Rows.LastOrDefault()?.SubAccount);
        Assert.Equal(command.Rows.LastOrDefault()?.Amount, updatedBooking?.Rows.LastOrDefault()?.Amount);
        Assert.Equal(command.Rows.LastOrDefault()?.CostCenter, updatedBooking?.Rows.LastOrDefault()?.CostCenter);
        await _checkpoint.Reset(Utils.ConnectionString);
    }

    [Fact]
    public async void DeleteBookingHandler_Pass()
    {
        // Arrange
        var auth = new TestAuthService();
        var dateTime = Utils.GetDateTimeService(new DateTime(2021, 12, 1));
        var context = Utils.GetDbContext(auth, dateTime);
        var testBooking = GetTestBooking(dateTime.GetUtcDateTime());
        await context.Bookings.AddAsync(testBooking);
        await context.SaveChangesAsync();

        var command = new DeleteBookingCommand
        {
            BookingId = testBooking.Id            
        };
        var handler = new DeleteBookingCommand.DeleteBookingHandler(context, auth);
        var validator = new DeleteBookingCommand.DeleteBookingValidation();

        // Act
        var validateRes = await validator.ValidateAsync(command);
        await handler.Handle(command, new CancellationToken());

        // Assert
        Assert.True(validateRes.IsValid);
        Assert.Empty(context.Bookings.ToList());
        Assert.Empty(context.BookingRows.ToList());

        await _checkpoint.Reset(Utils.ConnectionString);
    }

    private static Booking GetTestBooking(DateTime bookingDate)
    {
        var booking = Utils.GetBaseBooking(bookingDate);
        booking.Rows = new BookingRow[]
        {
            Utils.GetBaseBookingRow("1010", 100, "123AB", "12345"),
            Utils.GetBaseBookingRow("2100", -100)
        };

        return booking;
    }


    private static readonly Checkpoint _checkpoint = new()
    {
        TablesToIgnore = new[]
        {
            "Accounts"
        },    
    };
}
