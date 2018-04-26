using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNoise.Xna.Generator
{
    public class XYZ : ModuleBase
    {
        private string Params { get; set; }    

        public XYZ(string paramz = "x") : base(0)
        {
            Params = string.Join("", (from c in paramz
                                      where "xyz".Contains(c)
                                      select c).Distinct());

            if (paramz.Length != 1)
            {
                paramz = "x";
            }
            Params = paramz;
        }

        public override double GetValue(double x, double y, double z)
        {
            switch (Params)
            {
                case "x":
                    return x;
                case "y":
                    return y;
                case "z":
                    return z;
                default:
                    return x;
            }
        }
    }
}
