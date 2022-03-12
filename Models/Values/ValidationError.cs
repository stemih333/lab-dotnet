namespace Models.Values;

public class ValidationError
{
    public string? Property { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AttemptedValue { get; set; }
}