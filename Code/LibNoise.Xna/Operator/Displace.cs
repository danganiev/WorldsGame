
namespace LibNoise.Xna.Operator
{
	/// <summary>
	/// Provides a noise module that uses three source modules to displace each
	/// coordinate of the input value before returning the output value from
	/// a source module. [OPERATOR]
	/// </summary>
	public class Displace : ModuleBase
	{
		/// <summary>
		/// Initializes a new instance of Displace.
		/// </summary>
		public Displace()
			: base(4)
		{
		}

		/// <summary>
		/// Initializes a new instance of Displace.
		/// </summary>
		/// <param name="input">The input module.</param>
		/// <param name="x">The displacement module of the x-axis.</param>
		/// <param name="y">The displacement module of the y-axis.</param>
		/// <param name="z">The displacement module of the z-axis.</param>
		public Displace(ModuleBase input, ModuleBase x, ModuleBase y, ModuleBase z)
			: base(4)
		{
			modules[0] = input;
			modules[1] = x;
			modules[2] = y;
			modules[3] = z;
		}

		/// <summary>
		/// Gets or sets the controlling module on the x-axis.
		/// </summary>
		public ModuleBase X
		{
			get { return modules[1]; }
			set
			{
				System.Diagnostics.Debug.Assert(value != null);
				modules[1] = value;
			}
		}

		/// <summary>
		/// Gets or sets the controlling module on the z-axis.
		/// </summary>
		public ModuleBase Y
		{
			get { return modules[2]; }
			set
			{
				System.Diagnostics.Debug.Assert(value != null);
				modules[2] = value;
			}
		}

		/// <summary>
		/// Gets or sets the controlling module on the z-axis.
		/// </summary>
		public ModuleBase Z
		{
			get { return modules[3]; }
			set
			{
				System.Diagnostics.Debug.Assert(value != null);
				modules[3] = value;
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
			System.Diagnostics.Debug.Assert(modules[0] != null);
			System.Diagnostics.Debug.Assert(modules[1] != null);
			System.Diagnostics.Debug.Assert(modules[2] != null);
			System.Diagnostics.Debug.Assert(modules[3] != null);
			double dx = x + modules[1].GetValue(x, y, z);
			double dy = y + modules[1].GetValue(x, y, z);
			double dz = z + modules[1].GetValue(x, y, z);
			return modules[0].GetValue(dx, dy, dz);
		}
	}
}