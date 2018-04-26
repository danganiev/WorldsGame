﻿using System;

namespace LibNoise.Xna.Operator
{
	/// <summary>
	/// Provides a noise module that maps the output value from a source module onto an
	/// exponential curve. [OPERATOR]
	/// </summary>
	public class Exponent : ModuleBase
	{
		private double _exponent = 1.0;

		/// <summary>
		/// Initializes a new instance of Exponent.
		/// </summary>
		public Exponent()
			: base(1)
		{
		}

		/// <summary>
		/// Initializes a new instance of Exponent.
		/// </summary>
		/// <param name="exponent">The exponent to use.</param>
		/// <param name="input">The input module.</param>
		public Exponent(double exponent, ModuleBase input)
			: base(1)
		{
			modules[0] = input;
			Value = exponent;
		}

		/// <summary>
		/// Gets or sets the exponent.
		/// </summary>
		public double Value
		{
			get { return _exponent; }
			set { _exponent = value; }
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
			double v = modules[0].GetValue(x, y, z);
			return (Math.Pow(Math.Abs((v + 1.0) / 2.0), _exponent) * 2.0 - 1.0);
		}
	}
}