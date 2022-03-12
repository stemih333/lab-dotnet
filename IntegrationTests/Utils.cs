using AppLogic;
using AutoMapper;
using Common.Interfaces;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Models.Enums;
using Moq;
using System;

namespace IntegrationTests
{
    public static class Utils
    {
        public const string ConnectionString = "Data Source=localhost,1433;Initial Catalog=BookingsDb_testing;Integrated Security=False;User ID=sa;Password=Sommaren1984!;";
        public static BookingContext GetDbContext(TestAuthService authService, IDateTimeService dateTimeService)
        {
            var options = GetDbOptions(ConnectionString);
            return new BookingContext(options, authService, dateTimeService);
        }

        public static DbContextOptions GetDbOptions(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BookingContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return optionsBuilder.Options;
        }

        public static IMapper GetAutoMapper()
        {
            var config = new MapperConfiguration(_ => _.AddProfile<AutomapperProfile>());
            return config.CreateMapper();
        }

        public static IDateTimeService GetDateTimeService(DateTime date)
        {
            var dateTimeService = new Mock<IDateTimeService>();
            dateTimeService.Setup(_ => _.GetUtcDateTime()).Returns(date);

            return dateTimeService.Object;
        }

        public static Booking GetBaseBooking(DateTime? bookingDate = default)
        {
            return new Booking()
            {
                BookingDate = bookingDate,
                BookingStatus = BookingStatus.Saved,
            };
        }

        public static BookingRow GetBaseBookingRow(string? account = default, decimal? amount = default, string? costCenter = default, string? subAccount = default)
        {
            return new BookingRow
            {
                Account = account,
                Amount = amount,
                CostCenter = costCenter,
                SubAccount = subAccount,
            };
        }
    }
}