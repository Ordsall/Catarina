using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public interface IRadarModule
    {
        void GetData();

        string Serial { get; }

        string Type { get; set; }


    }
}
