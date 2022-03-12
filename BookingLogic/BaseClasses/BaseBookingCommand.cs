namespace AppLogic.BaseClasses
{
    public class BaseBookingCommand
    {
        public int? BookingId { get; set; }
        public DateTime? BookingDate { get; set; }
        public string? Approver { get; set; }
        public List<BookingRowDto> Rows { get; set; }
    }
}