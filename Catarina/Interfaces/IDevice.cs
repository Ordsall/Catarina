using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    /// <summary>
    /// Testing device interface
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Get data from device
        /// </summary>
        /// <param name="progress">Data reciving progress</param>
        /// <returns></returns>
        IEnumerable<IParameter> GetData(IProgress<string> progress = null);

        /// <summary>
        /// (Deprecated) Get list of string headers for the parameters
        /// </summary>
        /// <returns>List of string headers</returns>
        [Obsolete("GetHeaders() is deprecated, please use GetData() with parameters names instead.")]
        List<string> GetHeaders();

        /// <summary>
        /// Device serial number
        /// </summary>
        string SerialNumber { get; }

        /// <summary>
        /// Device type
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Device connection settings
        /// </summary>
        ISettings Settings { get; set; }

        /// <summary>
        /// Connect device
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect device
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Is Device connected
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Get device measured speed value
        /// </summary>
        double Speed { get; }


    }

    /// <summary>
    /// Args of ParametersChanged event
    /// </summary>
    public class ParametersChangedArgs : EventArgs
    {
        /// <summary>
        /// Recived measured parameters values
        /// </summary>
        public List<IParameter> Parameters = new List<IParameter>();
    }

    /// <summary>
    /// Implements if device supports the FlowMode (automatically recive masured parameters)
    /// </summary>
    public interface IFlowable
    {
        /// <summary>
        /// Enable flow mod
        /// </summary>
        void EnableFlow();

        /// <summary>
        /// Raise when device measured parameters is changed
        /// </summary>
        event EventHandler ParametersChanged;

        /// <summary>
        /// (Deprecated) Get list of string headers for the parameters in flow mode
        /// </summary>
        /// <returns>List of string headers</returns>
        [Obsolete("GetFlowHeaders() is deprecated, please use ParametersChanged event with parameters names instead.")]
        List<string> GetFlowHeaders();

        /// <summary>
        /// Disable flow mode
        /// </summary>
        void DisableFlow();
    }

    /// <summary>
    /// Implements if device supports calculation
    /// </summary>
    public interface ISpectrum
    {
        /// <summary>
        /// Get current spectrum vlaues (default 512 points)
        /// </summary>
        /// <returns>Spectrum</returns>
        IEnumerable<double> GetSpectrum();

        /// <summary>
        /// Calculte noize value in spectrum data
        /// </summary>
        /// <returns>Noize value</returns>
        double GetSignal();

        /// <summary>
        /// Calculte signal value in spectrum data
        /// </summary>
        /// <returns>Signal value</returns>
        double GetNoize();
    }
}
