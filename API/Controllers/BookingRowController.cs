namespace API.Controllers;

public class BookingRowController : BaseController
{
    [HttpPost]
    public async Task<IdResultDto> Post([FromBody] NewBookingRowCommand model) => await Mediator.Send(model);

    [HttpDelete("{rowId:int?}")]
    public async Task Delete(int? rowId) => await Mediator.Send(new DeleteBookingRowCommand { RowId = rowId });
}
