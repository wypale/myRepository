using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mathProject
{
    public class MyMath
    {
        private static bool GetDecimalSep()
        {
            string str = (1.0 / 2).ToString();
            str = str.Substring(1, 1);
            if (str == ".")
                return true;
            else
                return false;
        }

        internal static double ifDecSepDbl(string sInput)
        {
            if (GetDecimalSep())
                return Convert.ToDouble(sInput.Replace(",", "."));
            else
                return Convert.ToDouble(sInput.Replace(".", ","));
        }
        public static double triangleArea(double a, double b)
        {
            return 1.0 / 2.0 * a * b;
        }
    }
}
