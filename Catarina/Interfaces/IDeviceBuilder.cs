﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public interface IDeviceBuilder
    {
        IRadarModule Build();

        string Type { get; }

        string SettingsStirng { get; }
    }
}
