using Microsoft.Xna.Framework;

namespace LibNoise.Xna.Generator
{
    /// <summary>
    /// Provides a noise module that outputs a three-dimensional perlin noise. [GENERATOR]
    /// </summary>
    public class Perlin : ModuleBase
    {
        private int _octaveCount;

        /// <summary>
        /// Initializes a new instance of Perlin.
        /// </summary>
        /// <param name="frequency">The frequency of the first octave.</param>
        /// <param name="lacunarity">The lacunarity of the perlin noise.</param>
        /// <param name="persistence">The persistence of the perlin noise.</param>
        /// <param name="octaves">The number of octaves of the perlin noise.</param>
        /// <param name="seed">The seed of the perlin noise.</param>
        /// <param name="quality">The quality of the perlin noise.</param>
        public Perlin(double frequency = 1.0, double lacunarity = 2.0, double persistence = 0.5, int octaves = 6, int seed = 0,
            QualityMode quality = QualityMode.Medium)
            : base(0)
        {
            Frequency = frequency;
            Lacunarity = lacunarity;
            OctaveCount = octaves;
            Persistence = persistence;
            Seed = seed;
            Quality = quality;
        }

        /// <summary>
        /// Gets or sets the frequency of the first octave.
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// Gets or sets the lacunarity of the perlin noise.
        /// </summary>
        public double Lacunarity { get; set; }

        /// <summary>
        /// Gets or sets the quality of the perlin noise.
        /// </summary>
        public QualityMode Quality { get; set; }

        /// <summary>
        /// Gets or sets the number of octaves of the perlin noise.
        /// </summary>
        public int OctaveCount
        {
            get { return _octaveCount; }
            set { _octaveCount = (int)MathHelper.Clamp(value, 1, Utils.OctavesMaximum); }
        }

        /// <summary>
        /// Gets or sets the persistence of the perlin noise.
        /// </summary>
        public double Persistence { get; set; }

        /// <summary>
        /// Returns the output value for the given input coordinates.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <param name="z">The input coordinate on the z-axis.</param>
        /// <returns>The resulting output value.</returns>
        public override double GetValue(double x, double y, double z)
        {
            double value = 0.0;
            double signal;
            double cp = 1.0;
            double nx, ny, nz;
            long seed;
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;
            for (int i = 0; i < _octaveCount; i++)
            {
                nx = Utils.MakeInt32Range(x);
                ny = Utils.MakeInt32Range(y);
                nz = Utils.MakeInt32Range(z);
                seed = (Seed + i) & 0xffffffff;
                double scale = 1.0 / (i + 2);
                signal = Utils.GradientCoherentNoise3D(nx * scale, ny * scale, nz * scale, seed, Quality);
                value += signal * cp;
                x *= Lacunarity;
                y *= Lacunarity;
                z *= Lacunarity;
                cp *= Persistence;
            }
            return value;
        }
    }
}