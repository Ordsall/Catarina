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

        public void WriteRow(DateTime dateTime, params double[] values)
        {
            lock (fileName)
            {
                string tcsv = string.Empty;
                tcsv += dateTime.ToString("dd-MM-yyyy HH.mm.ss") + ";";
                foreach (var val in values) { tcsv += val + ";"; }
                tcsv += "\n";
                System.IO.File.AppendAllText(fileName, tcsv);
            }
        }

        public Dictionary<DateTime, double[]> ReadAll()
        {
            var l = new Dictionary<DateTime, double[]>();
            string s = string.Empty;
            lock(fileName) { s = System.IO.File.ReadAllText(fileName); }
            string[] rows = s.Split('\n');
            foreach (var row in rows)
            {
                if (row.Length > 0)
                {
                    string[] string_values = row.Remove(row.Length - 1).Split(';');
                    var dt = DateTime.ParseExact(string_values[0], "dd-MM-yyyy HH.mm.ss", null);
                    double[] values = new double[string_values.Length-1];
                    for (int i = 0; i < values.Length; i++)
                    {
                        try { values[i] = Convert.ToDouble(string_values[i+1]); }
                        catch (Exception) { values[i] = double.NaN; }
                    }
                    l.Add(dt, values);
                }
            }
            return l;
        }


    }
}
