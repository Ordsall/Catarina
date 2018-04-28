using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    /// <summary>
    /// Device factory interface
    /// </summary>
    public abstract class IDeviceFactory
    {
        /// <summary>
        /// Build IDevice
        /// </summary>
        /// <returns>Device</returns>
        public abstract IDevice Build();

        /// <summary>
        /// Type of the device
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Device information string presentation 
        /// </summary>
        public abstract string DeviceInfo { get; }

        /// <summary>
        /// Device connection settings
        /// </summary>
        public abstract ISettings Settings { get; set; }

    }
}
