using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{


    public interface IImitator 
    {
        string Serial { get; }

        string Type { get; }

        void Connect();

        void Disconnect();

        ISettings Settings { get; }

        bool IsConnected { get; }

        bool IsEnabled { get; }

        void Enable();

        void Disable();

        ViewModel.Direction Direction { get; set; }

        double Distance { get; set; }

        double Speed { get; set; }

        
    }
}
