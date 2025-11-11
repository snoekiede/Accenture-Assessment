namespace Accenture_Assessment.Contracts.Dtos;

public class SharedHolidayDto
{
    public DateTime Date { get; set; }
    public required string Country1Code { get; set; }
    public required string Country1LocalName { get; set; }
    public required string Country2Code { get; set; }
    public required string Country2LocalName { get; set; }
}
