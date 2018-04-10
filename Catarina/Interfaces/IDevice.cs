using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public interface IDevice
    {

        Dictionary<string, object> GetData(IProgress<string> progress = null);

        string SerialNumber { get; }

        string Type { get; }

        ISettings Settings { get; set; }

        void Connect();

        void Disconnect();

        bool IsConnected { get; }

        double Speed { get; }

        
    }
}
