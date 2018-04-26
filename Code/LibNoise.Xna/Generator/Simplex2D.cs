using System;
using System.Linq;

using Microsoft.Xna.Framework;

//TODO: Doesn't work yet
namespace LibNoise.Xna.Generator
{
    /// <summary>
    /// Provides a noise module that outputs a three-dimensional simplex noise. [GENERATOR]
    /// </summary>
    public class Simplex2D : ModuleBase
    {
        private OpenSimplexNoise _noise;

        /// <summary>
        /// Initializes a new instance of Simplex noise.
        /// </summary>
        /// <param name="seed">The seed of the perlin noise.</param>
        public Simplex2D(double scale = 0.0, int seed = 0, string paramz = "xz")
            : base(0)
        {
            Seed = seed;
            Scale = scale;
            Params = string.Join("", (from c in paramz
                                      where "xyz".Contains(c)
                                      select c).Distinct());

            if (paramz.Length != 2)
            {
                paramz = "xz";
            }
            Params = paramz;
        }

        private int _seed;

        private string Params { get; set; }

        private double Scale { get; set; }

        public override int Seed
        {
            get { return _seed; }
            set
            {
                _seed = value;
                _noise = new OpenSimplexNoise(_seed);
            }
        }

        /// <summary>
        /// Returns the output value for the given input coordinates.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <param name="z">The input coordinate on the z-axis.</param>
        /// <returns>The resulting output value.</returns>
        public override double GetValue(double x, double y, double z)
        {
            double nx = x * Scale;
            double ny = y * Scale;
            double nz = z * Scale;

            switch (Params)
            {
                case "xy":
                    return _noise.Noise(nx, ny);
                case "xz":
                    return _noise.Noise(nx, nz);
                case "yx":
                    return _noise.Noise(ny, nx);
                case "yz":
                    return _noise.Noise(ny, nz);
                case "zx":
                    return _noise.Noise(nz, nx);
                case "zy":
                    return _noise.Noise(nz, ny);
                default:
                    return _noise.Noise(nx, nz);
            }
        }
    }
}