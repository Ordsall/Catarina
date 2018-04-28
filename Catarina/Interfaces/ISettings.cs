using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    /// <summary>
    /// Interface of universal connection settings
    /// </summary>
    public interface ISettings
    {
    }

    /// <summary>
    /// Settings for serial devices
    /// </summary>
    public class SerialSettings : ISettings
    {
        /// <summary>
        /// Settings for serial devices
        /// </summary>
        /// <param name="PortName">Com-port name</param>
        public SerialSettings(string PortName) => this.PortName = PortName;

        /// <summary>
        /// Com-port name
        /// </summary>
        public string PortName { get; set; }
    }

}
