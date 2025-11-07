using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accenture_Assessment.Contracts.Enums;


namespace Accenture_Assessment.Data.Models
{
    public class Holiday
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public required string LocalName { get; set; }
        public required string Name { get; set; }
        public required string CountryCode { get; set; }
        public bool Fixed { get; set; } = false;
        public bool Global { get; set; } = false;
        public List<string> Counties { get; set; } = [];
        public int? LaunchYear { get; set; }
        public HolidayType Type { get; set; }
    }
}
