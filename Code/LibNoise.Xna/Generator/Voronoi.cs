using System;

namespace LibNoise.Xna.Generator
{
    /// <summary>
    /// Provides a noise module that outputs Voronoi cells. [GENERATOR]
    /// </summary>
    public class Voronoi : ModuleBase
    {
        private double _displacement = 1.0;
        private double _frequency = 1.0;
        private bool _distance;

        /// <summary>
        /// Initializes a new instance of Voronoi.
        /// </summary>
        public Voronoi()
            : base(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of Voronoi.
        /// </summary>
        /// <param name="frequency">The frequency of the first octave.</param>
        /// <param name="displacement">The displacement of the ridged-multifractal noise.</param>
        /// <param name="seed">The seed of the ridged-multifractal noise.</param>
        /// <param name="distance">Indicates whether the distance from the nearest seed point is applied to the output value.</param>
        public Voronoi(double frequency = 0, double displacement = 0, int seed = 0, bool distance = false)
            : base(0)
        {
            Frequency = frequency;
            Displacement = displacement;
            Seed = seed;
            UseDistance = distance;
        }

        /// <summary>
        /// Gets or sets the displacement value of the Voronoi cells.
        /// </summary>
        public double Displacement
        {
            get { return _displacement; }
            set { _displacement = value; }
        }

        /// <summary>
        /// Gets or sets the frequency of the seed points.
        /// </summary>
        public double Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }

        /// <summary>
        /// Gets or sets a value whether the distance from the nearest seed point is applied to the output value.
        /// </summary>
        public bool UseDistance
        {
            get { return _distance; }
            set { _distance = value; }
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
            int xi = (x > 0.0 ? (int)x : (int)x - 1);
            int iy = (y > 0.0 ? (int)y : (int)y - 1);
            int iz = (z > 0.0 ? (int)z : (int)z - 1);
            double md = 2147483647.0;
            double xc = 0;
            double yc = 0;
            double zc = 0;
            for (int zcu = iz - 2; zcu <= iz + 2; zcu++)
            {
                for (int ycu = iy - 2; ycu <= iy + 2; ycu++)
                {
                    for (int xcu = xi - 2; xcu <= xi + 2; xcu++)
                    {
                        double xp = xcu + Utils.ValueNoise3D(xcu, ycu, zcu, Seed);
                        double yp = ycu + Utils.ValueNoise3D(xcu, ycu, zcu, Seed + 1);
                        double zp = zcu + Utils.ValueNoise3D(xcu, ycu, zcu, Seed + 2);
                        double xd = xp - x;
                        double yd = yp - y;
                        double zd = zp - z;
                        double d = xd * xd + yd * yd + zd * zd;
                        if (d < md)
                        {
                            md = d;
                            xc = xp;
                            yc = yp;
                            zc = zp;
                        }
                    }
                }
            }
            double v;

            if (_distance)
            {
                double xd = xc - x;
                double yd = yc - y;
                double zd = zc - z;
                v = (Math.Sqrt(xd * xd + yd * yd + zd * zd)) * Utils.Sqrt3 - 1.0;
            }
            else
            {
                v = 0.0;
            }

            return v + (_displacement * Utils.ValueNoise3D((int)(Math.Floor(xc)), (int)(Math.Floor(yc)),
              (int)(Math.Floor(zc)), 0));
        }
    }
}