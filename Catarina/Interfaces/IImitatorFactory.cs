using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    /// <summary>
    /// Imitator fatroty interface
    /// </summary>
    public interface IImitatorFactory
    {
        /// <summary>
        /// Build IImitator with factory settings
        /// </summary>
        /// <returns></returns>
        IImitator Build();
        
        /// <summary>
        /// Imitator type
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Device information string presentation 
        /// </summary>
        string DeviceInfo { get; }

        /// <summary>
        /// Device connection settings
        /// </summary>
        ISettings Settings { get; set; }

        /// <summary>
        /// Fatory default imitation speed value 
        /// </summary>
        double Speed { get; set; }

        /// <summary>
        /// Fatory default imitation distance value 
        /// </summary>
        double Distance { get; set; }

        /// <summary>
        /// Fatory default imitation direction value 
        /// </summary>
        ViewModel.Direction Direction { get; set; }
    }
}
