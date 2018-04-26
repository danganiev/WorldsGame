﻿namespace LibNoise.Xna.Operator
{
	/// <summary>
	/// Provides a noise module that applies a scaling factor and a bias to the output
	/// value from a source module. [OPERATOR]
	/// </summary>
	public class ScaleBias : ModuleBase
	{

		private double _scale = 1.0;
		private double _bias;

		/// <summary>
		/// Initializes a new instance of ScaleBias.
		/// </summary>
		public ScaleBias()
			: base(1)
		{
		}

		/// <summary>
		/// Initializes a new instance of ScaleBias.
		/// </summary>
		/// <param name="scale">The scaling factor to apply to the output value from the source module.</param>
		/// <param name="bias">The bias to apply to the scaled output value from the source module.</param>
		/// <param name="input">The input module.</param>
		public ScaleBias(double scale, double bias, ModuleBase input)
			: base(1)
		{
			modules[0] = input;
			Bias = bias;
			Scale = scale;
		}

		/// <summary>
		/// Gets or sets the bias to apply to the scaled output value from the source module.
		/// </summary>
		public double Bias
		{
			get { return _bias; }
			set { _bias = value; }
		}

		/// <summary>
		/// Gets or sets the scaling factor to apply to the output value from the source module.
		/// </summary>
		public double Scale
		{
			get { return _scale; }
			set { _scale = value; }
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
			return modules[0].GetValue(x, y, z) * _scale + _bias;
		}
	}
}