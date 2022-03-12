namespace API.Controllers;

public class BookingController : BaseController
{
    [HttpGet]
    public async Task<SearchResultDto<SearchBookingDto>> Search([FromQuery] SearchBookingsQuery model) => await Mediator.Send(model);

    [HttpGet("{bookingId:int}")]
    public async Task<BookingDto> Get(int bookingId) => await Mediator.Send(new GetBookingByIdQuery { BookingId = bookingId });

    [HttpPost]
    public async Task<IdResultDto> Post([FromBody] NewBookingCommand model) => await Mediator.Send(model);

    [HttpPut]
    public async Task Put([FromBody] UpdateBookingCommand model) => await Mediator.Send(model);

    [HttpDelete("{bookingId:int}")]
    public async Task Delete(int bookingId) => await Mediator.Send(new DeleteBookingCommand { BookingId = bookingId });
}
