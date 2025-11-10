using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accenture_Assessment.Contracts.Dtos
{
    public class HolidayResultDto
    {
        public required string Name { get; set; }
        public required DateTime Date { get; set; }
        public required string LocalName { get; set; }
    }
}
