using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Components
{
    public class CsvFile
    {
        string fileName = string.Empty;

        public CsvFile(string FileName)
        {
            fileName = FileName;
        }

        public void WriteHeaders(params string[] values)
        {
            lock (fileName)
            {
                string tcsv = string.Empty;
                foreach (var val in values) { tcsv += val + ";"; }
                tcsv += "\n";
                System.IO.File.WriteAllText(fileName, tcsv);
            }
        }

        public void Write(params double[] values)
        {
            lock (fileName)
            {
                string tcsv = string.Empty;
                foreach (var val in values) { tcsv += val + ";"; }
                tcsv += "\n";
                System.IO.File.AppendAllText(fileName, tcsv);
            }
        }
    }
}
