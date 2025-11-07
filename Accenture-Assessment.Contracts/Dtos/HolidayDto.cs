using Accenture_Assessment.Contracts.Enums;

namespace Accenture_Assessment.Contracts.Dtos;

public class HolidayDto
{
    public DateTime Date { get; set; }
    public required string LocalName { get; set; }
    public required string Name { get; set; }
    public required string CountryCode { get; set; }
    public bool Fixed { get; set; } = false;
    public bool Global { get; set; } = false;
    public List<string> Counties { get; set; } = [];
    public int? LaunchYear { get; set; }

    public HolidayType Type { get; set; } = HolidayType.Public;
}