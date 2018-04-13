using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public interface IDevice
    {

        IEnumerable<IParameter> GetData(IProgress<string> progress = null);

        List<string> GetHeaders();

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
        public List<IParameter> Parameters = new List<IParameter>();
    }

    public interface IFlowable
    {
        void EnableFlow();

        event EventHandler ParametersChanged;

        List<string> GetFlowHeaders();

        void DisableFlow();
    }

    public interface ISpectrum
    {
        IEnumerable<double> GetSpectrum();

        double GetSignal();

        double GetNoize();
    }
}
