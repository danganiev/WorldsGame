namespace LibNoise.Xna.Operator
{
    /// <summary>
    /// Provides a noise module that outputs the value selected from one of two source
    /// modules chosen by the output value from a control module. [OPERATOR]
    /// </summary>
    public class Select : ModuleBase
    {
        private double _fallOff;
        private double _raw;
        private double _min = -1.0;
        private double _max = 1.0;

        /// <summary>
        /// Initializes a new instance of Select.
        /// </summary>
        public Select()
            : base(3)
        {
        }

        /// <summary>
        /// Initializes a new instance of Select.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="fallOff">The falloff value at the edge transition.</param>
        /// <param name="input1">The first input module.</param>
        /// <param name="input2">The second input module.</param>
        /// <param name="controller">The controller of the operator.</param>
        public Select(double min, double max, double fallOff, ModuleBase input1, ModuleBase input2, ModuleBase controller)
            : base(3)
        {
            modules[0] = input1;
            modules[1] = input2;
            modules[2] = controller;
            _min = min;
            _max = max;
            FallOff = fallOff;
        }

        /// <summary>
        /// Gets or sets the controlling module.
        /// </summary>
        public ModuleBase Controller
        {
            get { return modules[2]; }
            set
            {
                System.Diagnostics.Debug.Assert(value != null);
                modules[2] = value;
            }
        }

        /// <summary>
        /// Gets or sets the falloff value at the edge transition.
        /// </summary>
        public double FallOff
        {
            get { return _fallOff; }
            set
            {
                double bs = _max - _min;
                _raw = value;
                _fallOff = (value > bs / 2) ? bs / 2 : value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        public double Maximum
        {
            get { return _max; }
            set
            {
                _max = value;
                FallOff = _raw;
            }
        }

        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        public double Minimum
        {
            get { return _min; }
            set
            {
                _min = value;
                FallOff = _raw;
            }
        }

        /// <summary>
        /// Sets the bounds.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public void SetBounds(double min, double max)
        {
            System.Diagnostics.Debug.Assert(min < max);
            _min = min;
            _max = max;
            FallOff = _fallOff;
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
            System.Diagnostics.Debug.Assert(modules[1] != null);
            System.Diagnostics.Debug.Assert(modules[2] != null);
            double cv = modules[2].GetValue(x, y, z);
            if (_fallOff > 0.0)
            {
                if (cv < (_min - _fallOff)) { return modules[0].GetValue(x, y, z); }
            
                double a;
                
                if (cv < (_min + _fallOff))
                {
                    double lc = (_min - _fallOff);
                    double uc = (_min + _fallOff);
                    a = Utils.MapCubicSCurve((cv - lc) / (uc - lc));
                    return Utils.InterpolateLinear(modules[0].GetValue(x, y, z), modules[1].GetValue(x, y, z), a);

                }
                
                if (cv < (_max - _fallOff)) { return modules[1].GetValue(x, y, z); }
                
                if (cv < (_max + _fallOff))
                {
                    double lc = (_max - _fallOff);
                    double uc = (_max + _fallOff);
                    a = Utils.MapCubicSCurve((cv - lc) / (uc - lc));
                    return Utils.InterpolateLinear(modules[1].GetValue(x, y, z), modules[0].GetValue(x, y, z), a);

                }
                return modules[0].GetValue(x, y, z);
            }
            
            if (cv < _min || cv > _max) { return modules[0].GetValue(x, y, z); }
            return modules[1].GetValue(x, y, z);
        }
    }
}