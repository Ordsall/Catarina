using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    /// <summary>
    /// Measured parameter interface
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// Name of measured parameter
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// Double parameter
    /// </summary>
    public class DoubleParameter : IParameter
    {
        /// <summary>
        /// Double parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Measured parameter value</param>
        public DoubleParameter(string name, double value) { Value = value; Name = name; }

        /// <summary>
        /// Measured parameter value
        /// </summary>
        public double Value { get; private set; }

        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; private set; }
    }

    /// <summary>
    /// Spectrum parameter
    /// </summary>
    public class SpectrumsParameter : IParameter
    {
        /// <summary>
        /// Spectrums parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="spectrums">Calculated spectrum</param>
        public SpectrumsParameter(string name, List<double[]> spectrums) { Name = name; Spectrums = spectrums; }


        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Calculated spectrum
        /// </summary>
        public List<double[]> Spectrums { get; private set; }
    }
}
