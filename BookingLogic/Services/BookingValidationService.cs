using FluentValidation.Results;

namespace AppLogic.Services;

public class BookingValidationService : IBookingValidationService
{
    private readonly IDateTimeService _dateTimeService;
    public BookingValidationService(IDateTimeService dateTimeService)
    {
        _dateTimeService = dateTimeService;
    }

    public Task ValidateBookingAsync(Booking booking, IEnumerable<string> accounts)
    {
        var validator = new BookingValidator(accounts, _dateTimeService.GetUtcDateTime());
        var result = validator.Validate(booking);
        if (!result.IsValid) throw new AppValidationException(result.Errors);

        return Task.CompletedTask;
    }

    private class BookingValidator : AbstractValidator<Booking>
    {
        public BookingValidator(IEnumerable<string> accounts, DateTime now)
        {
            RuleFor(_ => _.Approver).MaximumLength(200);
            RuleFor(_ => _.BookingDate).NotEmpty().Custom((date, ctx) =>
            {
                var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                var twoMonthsInFuture = firstDayOfMonth.AddMonths(2);
                if (date < firstDayOfMonth)
                    ctx.AddFailure(new ValidationFailure("BookingDate", "A previous period cannot be used as booking date."));

                if (date >= twoMonthsInFuture)
                    ctx.AddFailure(new ValidationFailure("BookingDate", "Bookings cannot have period that is more than two months in future."));
            });
            RuleFor(_ => _.BookingStatus).NotNull();
            RuleFor(_ => _.Rows).Custom((rows, ctx) =>
            {
                if (!rows.Any())
                    ctx.AddFailure(new ValidationFailure("Rows", "Booking does not contain any rows."));
                foreach (var item in rows.Select((_, i) => new { Row = _, Index = i }))
                {
                    var rowNo = item.Index + 1;
                    var result = new BookingRowValidator(accounts, rowNo).Validate(item.Row);
                    if (!result.IsValid)
                        result.Errors.ForEach(_ => ctx.AddFailure(_));
                }

                if (rows.Count() == 1)
                {
                    ctx.AddFailure(new ValidationFailure("Rows", "Booking cannot contain only one row."));
                }

                if (rows.Any(_ => _.Amount == 0))
                    ctx.AddFailure(new ValidationFailure("Amount", "Booking cannot contain rows with zero amount."));
                else
                {
                    var bookingBalance = rows.Select(_ => _.Amount).Sum();
                    if (bookingBalance != 0)
                        ctx.AddFailure(new ValidationFailure("Amount", "All the booking rows do not balance."));
                }
            });
        }

        private class BookingRowValidator : AbstractValidator<BookingRow>
        {
            private readonly string _alphaNumeric  = @"^([a-zA-Z0-9])*[^\s]\1*$";
            private readonly string _numeric  = @"^([0-9])*[^\s]\1*$";
            public BookingRowValidator(IEnumerable<string> accounts, int rowNo)
            {
                RuleFor(_ => _.CostCenter).MaximumLength(20).Matches(_alphaNumeric).WithName($"Cost center (Row {rowNo})");
                RuleFor(_ => _.SubAccount).MaximumLength(10).Matches(_alphaNumeric).WithName($"Sub account (Row {rowNo})"); ;
                RuleFor(_ => _.Account).NotEmpty().Matches(_numeric).MaximumLength(5).WithName($"Account (Row {rowNo})").Custom((acc, ctx) =>
                {
                    if (!string.IsNullOrWhiteSpace(acc))
                    {
                        var accountExists = accounts.Any(_ => _ == acc);
                        if (!accountExists)
                            ctx.AddFailure(new ValidationFailure($"Account (Row {rowNo})", $"Account {acc} is not a valid account."));
                    }                   
                });
                RuleFor(_ => _.Amount).NotEmpty().ScalePrecision(2, 16).WithName($"Amount (Row {rowNo})"); ;
                RuleFor(_ => _.BookingId).NotEmpty().WithName($"Booking ID (Row {rowNo})"); ;
            }
        }
    }
}
