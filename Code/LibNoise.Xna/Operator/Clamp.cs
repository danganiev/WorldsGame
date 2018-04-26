
namespace LibNoise.Xna.Operator
{
    /// <summary>
    /// Provides a noise module that clamps the output value from a source module to a
    /// range of values. [OPERATOR]
    /// </summary>
    public class Clamp : ModuleBase
    {
        private double _min = -1.0;
        private double _max = 1.0;
        
        /// <summary>
        /// Initializes a new instance of Clamp.
        /// </summary>
        public Clamp()
            : base(1)
        {
        }

        /// <summary>
        /// Initializes a new instance of Clamp.
        /// </summary>
        /// <param name="input">The input module.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public Clamp(double min, double max, ModuleBase input)
            : base(1)
        {
            Minimum = min;
            Maximum = max;
            modules[0] = input;
        }

        /// <summary>
        /// Gets or sets the maximum to clamp to.
        /// </summary>
        public double Maximum
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets or sets the minimum to clamp to.
        /// </summary>
        public double Minimum
        {
            get { return _min; }
            set { _min = value; }
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
            System.Diagnostics.Debug.Assert(modules[0] != null);
            if (_min > _max)
            {
                double t = _min;
                _min = _max;
                _max = t;
            }
            double v = modules[0].GetValue(x, y, z);
            
            if (v < _min) { return _min; }

            if (v > _max) { return _max; }

            return v;
        }
    }
}