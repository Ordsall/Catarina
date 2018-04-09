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
    public class Sapsan3Factory : Interfaces.IImitatorFactory
    {
        public string Type => "Сапсан 3";

        public string InfoString => "Скдыщь";

        public IImitator Build()
        {
            throw new NotImplementedException();
        }
    }
    

    public class Sapsan3 : Catarina.Interfaces.IImitator
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
