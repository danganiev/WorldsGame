﻿namespace LibNoise.Xna.Operator
{
	/// <summary>
	/// Provides a noise module that inverts the output value from a source module. [OPERATOR]
	/// </summary>
	public class Invert : ModuleBase
	{
		/// <summary>
		/// Initializes a new instance of Invert.
		/// </summary>
		public Invert()
			: base(1)
		{
		}

		/// <summary>
		/// Initializes a new instance of Invert.
		/// </summary>
		/// <param name="input">The input module.</param>
		public Invert(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
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
			return -modules[0].GetValue(x, y, z);
		}
	}
}