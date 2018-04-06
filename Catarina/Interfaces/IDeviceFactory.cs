using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public abstract class IDeviceFactory
    {
        public abstract IRadarModule Build();

        public abstract string Type { get; }

        public abstract string DeviceInfo { get; }

    }
}
