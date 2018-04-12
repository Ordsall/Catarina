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

        Dictionary<string, int> GetHeaders();

        string SerialNumber { get; }

        string Type { get; }

        ISettings Settings { get; set; }

        void Connect();

        void Disconnect();

        bool IsConnected { get; }

        double Speed { get; }


    }

    public class ParametersChangedArgs : EventArgs
    {
        public Dictionary<int, double> Parameters = new Dictionary<int, double>();
    }

    public interface IFlowable
    {
        void EnableFlow();

        event EventHandler ParametersChanged;

        void DisableFlow();
    }

    public interface ISpectrum
    {
        IEnumerable<double> GetSpectrum();

        double GetSignal();

        double GetNoize();
    }
}
