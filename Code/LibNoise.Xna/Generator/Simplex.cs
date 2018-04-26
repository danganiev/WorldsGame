using System;
using System.Linq;

using Microsoft.Xna.Framework;

//TODO: Doesn't work yet
namespace LibNoise.Xna.Generator
{
    /// <summary>
    /// Provides a noise module that outputs a three-dimensional simplex noise. [GENERATOR]
    /// </summary>
    public class Simplex : ModuleBase
    {
        private OpenSimplexNoise _noise;

        private string Params { get; set; }

        /// <summary>
        /// Initializes a new instance of Simplex noise.
        /// </summary>
        /// <param name="frequency">The frequency of the first octave.</param>
        /// <param name="lacunarity">The lacunarity of the perlin noise.</param>
        /// <param name="persistence">The persistence of the perlin noise.</param>
        /// <param name="octaves">The number of octaves of the perlin noise.</param>
        /// <param name="seed">The seed of the perlin noise.</param>
        public Simplex(double scale = 0.0, int seed = 0, string paramz = "xyz")
            : base(0)
        {
            Seed = seed;
            Scale = scale;
            Params = string.Join("", (from c in paramz
                                      where "xyz".Contains(c)
                                      select c).Distinct());

            if (paramz.Length != 3)
            {
                paramz = "xyz";
            }
            Params = paramz;
        }

        private int _seed;
        private double Scale;

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
                case "xyz":
                    return _noise.Noise(nx, ny, nz);
                case "xzy":
                    return _noise.Noise(nx, nz, ny);
                case "yxz":
                    return _noise.Noise(ny, nx, nz);
                case "yzx":
                    return _noise.Noise(ny, nz, nx);
                case "zxy":
                    return _noise.Noise(nz, nx, ny);
                case "zyx":
                    return _noise.Noise(nz, ny, nx);
                default:
                    return _noise.Noise(nx, ny, nz);
            }
        }
    }
}