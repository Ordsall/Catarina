using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public interface IParameter
    {
        string Name { get; }
    }

    public class DoubleParameter : IParameter
    {
        public DoubleParameter(string name, double value) { Value = value; Name = name; }

        public double Value { get; private set; }

        public string Name { get; private set; }
    }

    public class SpectrumsParameter : IParameter
    {
        public SpectrumsParameter(string name, List<double[]> spectrums) { Name = name; Spectrums = spectrums; }

        public string Name { get; private set; }

        public List<double[]> Spectrums { get; private set; }
    }
}
