namespace LibNoise.Xna.Generator
{
    /// <summary>
    /// Provides a noise module that outputs a constant value. [GENERATOR]
    /// </summary>
    public class Random : ModuleBase
    {
        private int _value;
        private System.Random _random;

        /// <summary>
        /// Initializes a new instance of Const.
        /// </summary>
        public Random()
            : base(0)
        {
            _random = new System.Random(Seed);
        }

        /// <summary>
        /// Initializes a new instance of Const.
        /// </summary>
        /// <param name="value">The constant value.</param>
        public Random(int value)
            : base(0)
        {
            Value = value;
            _random = new System.Random(Seed);
        }

        /// <summary>
        /// Gets or sets the constant value.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set { _value = value; }
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
            return _random.Next(Value);
        }
    }
}