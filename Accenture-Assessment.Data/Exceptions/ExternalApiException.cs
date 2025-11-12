using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accenture_Assessment.Data.Exceptions
{
    public class ExternalApiException: Exception
    {
        public ExternalApiException()
        {
        }
        public ExternalApiException(string message)
            : base(message)
        {
        }
        public ExternalApiException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
