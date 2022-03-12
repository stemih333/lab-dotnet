namespace API.Controllers;

public class BookingStatusController : BaseController
{
    [HttpPost("saved")]
    public async Task SavedStatus([FromBody] int bookingId)
        => await Mediator.Send(new UpdateBookingStatusCommand { BookingId = bookingId, Status = BookingStatus.Saved });

    [HttpPost("cancelled")]
    public async Task CancelledStatus([FromBody] int bookingId)
        => await Mediator.Send(new UpdateBookingStatusCommand { BookingId = bookingId, Status = BookingStatus.Cancelled });

    [HttpPost("toBeApproved")]
    public async Task ToBeApprovedStatus([FromBody] int bookingId)
    {
        await Mediator.Send(new ValidateBookingCommand { BookingId = bookingId });
        await Mediator.Send(new UpdateBookingStatusCommand { BookingId = bookingId, Status = BookingStatus.ToBeApproved });
    }

    [HttpPost("toBeBooked")]
    public async Task ToBeBookedStatus([FromBody] int bookingId)
    {
        await Mediator.Send(new ValidateBookingCommand { BookingId = bookingId });
        await Mediator.Send(new UpdateBookingStatusCommand { BookingId = bookingId, Status = BookingStatus.ToBeBooked });
    }

    [HttpPost]
    public async Task Post([FromBody] UpdateBookingStatusCommand model)
    {
        if (model.Status == BookingStatus.ToBeApproved || model.Status == BookingStatus.ToBeBooked)
            await Mediator.Send(new ValidateBookingCommand { BookingId = model.BookingId });
        await Mediator.Send(model);

    }
}
