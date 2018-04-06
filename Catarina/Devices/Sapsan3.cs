using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catarina.Interfaces;
using Newtonsoft.Json;

namespace Catarina.Devices
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Sapsan3Factory : Interfaces.IImitatorFactory, ISerialDeviceFactory
    {
        public string Type => "Сапсан 3";

        [JsonProperty()]
        public string PortName { get; set; }

        public string InfoString => this.ToString();

        public IImitator Build() { return new Sapsan3(); }

        public override string ToString()
        {
            return String.Format("{0} ({1})", Type, PortName);
        }
    }

    public class Sapsan3 : Interfaces.IImitator, Interfaces.ISerialDevice
    {
        public string PortName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public uint BaudRate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Serial => "000001";

        public string Type => "Сапсан 3";

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Connect(string PortName)
        {
            throw new NotImplementedException();
        }
    }
}
