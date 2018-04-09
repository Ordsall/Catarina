using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public interface IImitatorFactory
    {
        IImitator Build();
        
        string Type { get; }
        
        string DeviceInfo { get; }
        
        ISettings Settings { get; set; }
    }
}
