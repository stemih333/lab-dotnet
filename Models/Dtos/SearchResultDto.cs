namespace Models.Dtos;

public class SearchResultDto<M>
{
    public IEnumerable<M> Results { get; set; }
    public int TotalNumberOfRows { get; set; }
    public byte[] ExportedData { get; set; }
}
