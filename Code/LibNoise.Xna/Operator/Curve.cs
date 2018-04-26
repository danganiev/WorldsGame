using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace LibNoise.Xna.Operator
{
	/// <summary>
	/// Provides a noise module that maps the output value from a source module onto an
	/// arbitrary function curve. [OPERATOR]
	/// </summary>
	public class Curve : ModuleBase
	{
		private List<KeyValuePair<double, double>> _data = new List<KeyValuePair<double, double>>();

		/// <summary>
		/// Initializes a new instance of Curve.
		/// </summary>
		public Curve()
			: base(1)
		{
		}

		/// <summary>
		/// Initializes a new instance of Curve.
		/// </summary>
		/// <param name="input">The input module.</param>
		public Curve(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
		}

		/// <summary>
		/// Gets the number of control points.
		/// </summary>
		public int ControlPointCount
		{
			get { return _data.Count; }
		}

		/// <summary>
		/// Gets the list of control points.
		/// </summary>
		public List<KeyValuePair<double, double>> ControlPoints
		{
			get { return _data; }
		}

		/// <summary>
		/// Adds a control point to the curve.
		/// </summary>
		/// <param name="input">The curves input value.</param>
		/// <param name="output">The curves output value.</param>
		public void Add(double input, double output)
		{
			var kvp = new KeyValuePair<double, double>(input, output);
			if (!_data.Contains(kvp))
			{
				_data.Add(kvp);
			}
			_data.Sort((lhs, rhs) => lhs.Key.CompareTo(rhs.Key));
		}

		/// <summary>
		/// Clears the control points.
		/// </summary>
		public void Clear()
		{
			_data.Clear();
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
			System.Diagnostics.Debug.Assert(ControlPointCount >= 4);
			double smv = modules[0].GetValue(x, y, z);
			int ip;
			for (ip = 0; ip < _data.Count; ip++)
			{
				if (smv < _data[ip].Key)
				{
					break;
				}
			}
			int i0 = (int)MathHelper.Clamp(ip - 2, 0, _data.Count - 1);
			int i1 = (int)MathHelper.Clamp(ip - 1, 0, _data.Count - 1);
			int i2 = (int)MathHelper.Clamp(ip, 0, _data.Count - 1);
			int i3 = (int)MathHelper.Clamp(ip + 1, 0, _data.Count - 1);
			if (i1 == i2)
			{
				return _data[i1].Value;
			}
			double ip0 = _data[i1].Key;
			double ip1 = _data[i2].Key;
			double a = (smv - ip0) / (ip1 - ip0);
			return Utils.InterpolateCubic(_data[i0].Value, _data[i1].Value, _data[i2].Value, _data[i3].Value, a);
		}
	}
}