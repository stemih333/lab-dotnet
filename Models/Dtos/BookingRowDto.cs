namespace Models.Dtos;

public class BookingRowDto
{
	public int? Id { get; set; }
	public string? CostCenter { get; set; }
	public string? Account { get; set; }
	public string? SubAccount { get; set; }
	public decimal? Amount { get; set; } = 0.00M;
}
