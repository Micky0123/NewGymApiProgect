using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class ServerBusyException:Exception
    {
        public ServerBusyException() : base("השרת עמוס כרגע ביצירת תוכניות אימונים. אנא המתן רגע ונסה שוב.") { }

        public ServerBusyException(string message) : base(message) { }

        public ServerBusyException(string message, Exception innerException) : base(message, innerException) { }

    }
}
