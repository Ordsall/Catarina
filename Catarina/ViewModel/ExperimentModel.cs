﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.ViewModel
{
    public class ExperimentModel : ViewModelBase
    {
        public string Type => Device.Type;

        public string Serial => Device.Serial;

        public Interfaces.IRadarModule Device { get; set; } = new Devices.Phasant();

        
    }
}
