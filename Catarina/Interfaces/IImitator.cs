using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{

    /// <summary>
    /// Imitator interface
    /// </summary>
    public interface IImitator 
    {
        /// <summary>
        /// Imitator serial number
        /// </summary>
        string Serial { get; }

        /// <summary>
        /// Imitator type
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Connect to device
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from device
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Device connection settings
        /// </summary>
        ISettings Settings { get; }

        /// <summary>
        /// Is device connected
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Is imitation enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Enable imitation
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable imitation
        /// </summary>
        void Disable();

        /// <summary>
        /// Imitated direction
        /// </summary>
        ViewModel.Direction Direction { get; set; }

        /// <summary>
        /// Imitated distance
        /// </summary>
        double Distance { get; set; }

        /// <summary>
        /// Imitated speed
        /// </summary>
        double Speed { get; set; }

        
    }
}
