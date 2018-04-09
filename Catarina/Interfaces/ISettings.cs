using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public interface ISettings
    {
    }

    public class SerialSettings : ISettings
    {
        public string PortName { get; set; }
    }

}
