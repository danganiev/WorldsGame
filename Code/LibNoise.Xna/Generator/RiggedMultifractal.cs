using System;

using Microsoft.Xna.Framework;

namespace LibNoise.Xna.Generator
{
    /// <summary>
    /// Provides a noise module that outputs 3-dimensional ridged-multifractal noise. [GENERATOR]
    /// </summary>
    public class RiggedMultifractal : ModuleBase
    {
        private double _frequency = 1.0;
        private double _lacunarity = 2.0;
        private QualityMode _quality = QualityMode.Medium;
        private int _octaveCount = 6;
        private readonly double[] _weights = new double[Utils.OctavesMaximum];

        /// <summary>
        /// Initializes a new instance of RiggedMultifractal.
        /// </summary>
        public RiggedMultifractal()
            : base(0)
        {
            UpdateWeights();
        }

        /// <summary>
        /// Initializes a new instance of RiggedMultifractal.
        /// </summary>
        /// <param name="frequency">The frequency of the first octave.</param>
        /// <param name="lacunarity">The lacunarity of the ridged-multifractal noise.</param>
        /// <param name="octaves">The number of octaves of the ridged-multifractal noise.</param>
        /// <param name="seed">The seed of the ridged-multifractal noise.</param>
        /// <param name="quality">The quality of the ridged-multifractal noise.</param>
        public RiggedMultifractal(double frequency = 1.0, double lacunarity = 2.0, int octaves = 6, int seed = 0, QualityMode quality = QualityMode.Medium)
            : base(0)
        {
            Frequency = frequency;
            Lacunarity = lacunarity;
            OctaveCount = octaves;
            Seed = seed;
            Quality = quality;
        }

        /// <summary>
        /// Gets or sets the frequency of the first octave.
        /// </summary>
        public double Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }

        /// <summary>
        /// Gets or sets the lacunarity of the ridged-multifractal noise.
        /// </summary>
        public double Lacunarity
        {
            get { return _lacunarity; }
            set
            {
                _lacunarity = value;
                UpdateWeights();
            }
        }

        /// <summary>
        /// Gets or sets the quality of the ridged-multifractal noise.
        /// </summary>
        public QualityMode Quality
        {
            get { return _quality; }
            set { _quality = value; }
        }

        /// <summary>
        /// Gets or sets the number of octaves of the ridged-multifractal noise.
        /// </summary>
        public int OctaveCount
        {
            get { return _octaveCount; }
            set { _octaveCount = (int)MathHelper.Clamp(value, 1, Utils.OctavesMaximum); }
        }

        /// <summary>
        /// Updates the weights of the ridged-multifractal noise.
        /// </summary>
        private void UpdateWeights()
        {
            double f = 1.0;
            for (int i = 0; i < Utils.OctavesMaximum; i++)
            {
                _weights[i] = Math.Pow(f, -1.0);
                f *= _lacunarity;
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
            x *= _frequency;
            y *= _frequency;
            z *= _frequency;
            double signal;
            double value = 0.0;
            double weight = 1.0;
            const double offset = 1.0;
            const double gain = 2.0;
            for (int i = 0; i < _octaveCount; i++)
            {
                double nx = Utils.MakeInt32Range(x);
                double ny = Utils.MakeInt32Range(y);
                double nz = Utils.MakeInt32Range(z);
                long seed = (Seed + i) & 0x7fffffff;
                signal = Utils.GradientCoherentNoise3D(nx, ny, nz, seed, _quality);
                signal = Math.Abs(signal);
                signal = offset - signal;
                signal *= signal;
                signal *= weight;
                weight = signal * gain;
                if (weight > 1.0) { weight = 1.0; }
                if (weight < 0.0) { weight = 0.0; }
                value += (signal * _weights[i]);
                x *= _lacunarity;
                y *= _lacunarity;
                z *= _lacunarity;
            }
            return (value * 1.25) - 1.0;
        }
    }
}