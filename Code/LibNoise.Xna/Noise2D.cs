using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LibNoise.Xna
{
    /// <summary>
    /// Provides a two-dimensional noise map.
    /// </summary>
    public class Noise2D : IDisposable
    {
        public const double SOUTH = -90.0;
        public const double NORTH = 90.0;
        public const double WEST = -180.0;
        public const double EAST = 180.0;
        public const double ANGLE_MIN = -180.0;
        public const double ANGLE_MAX = 180.0;
        public const double LEFT = -1.0;
        public const double RIGHT = 1.0;
        public const double TOP = -1.0;
        public const double BOTTOM = 1.0;

        private int _width;
        private int _height;
        private float _borderValue = float.NaN;
        private float[,] _data;
        private ModuleBase _generator;

        /// <summary>
        /// Initializes a new instance of Noise2D.
        /// </summary>
        protected Noise2D()
        {
        }

        /// <summary>
        /// Initializes a new instance of Noise2D.
        /// </summary>
        /// <param name="size">The width and height of the noise map.</param>
        public Noise2D(int size)
            : this(size, size, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of Noise2D.
        /// </summary>
        /// <param name="size">The width and height of the noise map.</param>
        /// <param name="generator">The generator module.</param>
        public Noise2D(int size, ModuleBase generator)
            : this(size, size, generator)
        {
        }

        /// <summary>
        /// Initializes a new instance of Noise2D.
        /// </summary>
        /// <param name="width">The width of the noise map.</param>
        /// <param name="height">The height of the noise map.</param>
        public Noise2D(int width, int height)
            : this(width, height, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of Noise2D.
        /// </summary>
        /// <param name="width">The width of the noise map.</param>
        /// <param name="height">The height of the noise map.</param>
        /// <param name="generator">The generator module.</param>
        public Noise2D(int width, int height, ModuleBase generator)
        {
            _generator = generator;
            _width = width;
            _height = height;
            _data = new float[width, height];
        }

        /// <summary>
        /// Gets or sets a value in the noise map by its position.
        /// </summary>
        /// <param name="x">The position on the x-axis.</param>
        /// <param name="y">The position on the y-axis.</param>
        /// <returns>The corresponding value.</returns>
        public float this[int x, int y]
        {
            get
            {
                if (x < 0 && x >= _width)
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (y < 0 && y >= _height)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return _data[x, y];
            }
            set
            {
                if (x < 0 && x >= _width)
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (y < 0 && y >= _height)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _data[x, y] = value;
            }
        }

        /// <summary>
        /// Gets or sets the constant value at the noise maps borders.
        /// </summary>
        public float Border
        {
            get { return _borderValue; }
            set { _borderValue = value; }
        }

        /// <summary>
        /// Gets or sets the generator module.
        /// </summary>
        public ModuleBase Generator
        {
            get { return _generator; }
            set { _generator = value; }
        }

        /// <summary>
        /// Gets the height of the noise map.
        /// </summary>
        public int Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Gets the width of the noise map.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }

        /// <summary>
        /// Clears the noise map.
        /// </summary>
        public void Clear()
        {
            Clear(0.0f);
        }

        /// <summary>
        /// Clears the noise map.
        /// </summary>
        /// <param name="value">The constant value to clear the noise map with.</param>
        public void Clear(float value)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _data[x, y] = value;
                }
            }
        }

        /// <summary>
        /// Generates a cylindrical projection of a point in the noise map.
        /// </summary>
        /// <param name="angle">The angle of the point.</param>
        /// <param name="height">The height of the point.</param>
        /// <returns>The corresponding noise map value.</returns>
        private double GenerateCylindrical(double angle, double height)
        {
            double x = Math.Cos(angle * Utils.DegToRad);
            double y = height;
            double z = Math.Sin(angle * Utils.DegToRad);
            return _generator.GetValue(x, y, z);
        }

        /// <summary>
        /// Generates a cylindrical projection of the noise map.
        /// </summary>
        /// <param name="angleMin">The maximum angle of the clip region.</param>
        /// <param name="angleMax">The minimum angle of the clip region.</param>
        /// <param name="heightMin">The minimum height of the clip region.</param>
        /// <param name="heightMax">The maximum height of the clip region.</param>
        public void GenerateCylindrical(double angleMin, double angleMax, double heightMin, double heightMax)
        {
            if (angleMax <= angleMin || heightMax <= heightMin || _generator == null)
            {
                throw new ArgumentException();
            }
            double ae = angleMax - angleMin;
            double he = heightMax - heightMin;
            double xd = ae / _width;
            double yd = he / _height;
            double ca = angleMin;
            for (int x = 0; x < _width; x++)
            {
                double ch = heightMin;
                for (int y = 0; y < _height; y++)
                {
                    _data[x, y] = (float)GenerateCylindrical(ca, ch);
                    ch += yd;
                }
                ca += xd;
            }
        }

        /// <summary>
        /// Generates a planar projection of a point in the noise map.
        /// </summary>
        /// <param name="x">The position on the x-axis.</param>
        /// <param name="y">The position on the y-axis.</param>
        /// <returns>The corresponding noise map value.</returns>
        private double GeneratePlanar(double x, double y)
        {
            return _generator.GetValue(x, 0.0, y);
        }

        /// <summary>
        /// Generates a planar projection of the noise map.
        /// </summary>
        /// <param name="left">The clip region to the left.</param>
        /// <param name="right">The clip region to the right.</param>
        /// <param name="top">The clip region to the top.</param>
        /// <param name="bottom">The clip region to the bottom.</param>
        public void GeneratePlanar(double left, double right, double top, double bottom)
        {
            GeneratePlanar(left, right, top, bottom, false);
        }

        /// <summary>
        /// Generates a non-seamless planar projection of the noise map.
        /// </summary>
        /// <param name="left">The clip region to the left.</param>
        /// <param name="right">The clip region to the right.</param>
        /// <param name="top">The clip region to the top.</param>
        /// <param name="bottom">The clip region to the bottom.</param>
        /// <param name="seamless">Indicates whether the resulting noise map should be seamless.</param>
        public void GeneratePlanar(double left, double right, double top, double bottom, bool seamless)
        {
            if (right <= left || bottom <= top || _generator == null)
            {
                throw new ArgumentException();
            }
            double xe = right - left;
            double ze = bottom - top;
            double xd = xe / _width;
            double zd = ze / _height;
            double xc = left;
            for (int x = 0; x < _width; x++)
            {
                double zc = top;
                for (int z = 0; z < _height; z++)
                {
                    float fv;
                    if (!seamless) { fv = (float)GeneratePlanar(xc, zc); }
                    else
                    {
                        double swv = GeneratePlanar(xc, zc);
                        double sev = GeneratePlanar(xc + xe, zc);
                        double nwv = GeneratePlanar(xc, zc + ze);
                        double nev = GeneratePlanar(xc + xe, zc + ze);
                        double xb = 1.0 - ((xc - left) / xe);
                        double zb = 1.0 - ((zc - top) / ze);
                        double z0 = Utils.InterpolateLinear(swv, sev, xb);
                        double z1 = Utils.InterpolateLinear(nwv, nev, xb);
                        fv = (float)Utils.InterpolateLinear(z0, z1, zb);
                    }
                    _data[x, z] = fv;
                    zc += zd;
                }
                xc += xd;
            }
        }

        /// <summary>
        /// Generates a spherical projection of a point in the noise map.
        /// </summary>
        /// <param name="lat">The latitude of the point.</param>
        /// <param name="lon">The longitude of the point.</param>
        /// <returns>The corresponding noise map value.</returns>
        private double GenerateSpherical(double lat, double lon)
        {
            double r = Math.Cos(Utils.DegToRad * lat);
            return _generator.GetValue(r * Math.Cos(Utils.DegToRad * lon), Math.Sin(Utils.DegToRad * lat),
                r * Math.Sin(Utils.DegToRad * lon));
        }

        /// <summary>
        /// Generates a spherical projection of the noise map.
        /// </summary>
        /// <param name="south">The clip region to the south.</param>
        /// <param name="north">The clip region to the north.</param>
        /// <param name="west">The clip region to the west.</param>
        /// <param name="east">The clip region to the east.</param>
        public void GenerateSpherical(double south, double north, double west, double east)
        {
            if (east <= west || north <= south || _generator == null)
            {
                throw new ArgumentException();
            }
            double loe = east - west;
            double lae = north - south;
            double xd = loe / _width;
            double yd = lae / _height;
            double clo = west;
            for (int x = 0; x < _width; x++)
            {
                double cla = south;
                for (int y = 0; y < _height; y++)
                {
                    _data[x, y] = (float)GenerateSpherical(cla, clo);
                    cla += yd;
                }
                clo += xd;
            }
        }

        /// <summary>
        /// Creates a normal map for the current content of the noise map.
        /// </summary>
        /// <param name="device">The graphics device to use.</param>
        /// <param name="scale">The scaling of the normal map values.</param>
        /// <returns>The created normal map.</returns>
        public Texture2D GetNormalMap(GraphicsDevice device, float scale)
        {
            var result = new Texture2D(device, _width, _height);
            var data = new Color[_width * _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    Vector3 normX = Vector3.Zero;
                    Vector3 normY = Vector3.Zero;
                    Vector3 normalVector = new Vector3();
                    if (x > 0 && y > 0 && x < _width - 1 && y < _height - 1)
                    {
                        normX = new Vector3((_data[x - 1, y] - _data[x + 1, y]) / 2 * scale, 0, 1);
                        normY = new Vector3(0, (_data[x, y - 1] - _data[x, y + 1]) / 2 * scale, 1);
                        normalVector = normX + normY;
                        normalVector.Normalize();
                        Vector3 texVector = Vector3.Zero;
                        texVector.X = (normalVector.X + 1) / 2f;
                        texVector.Y = (normalVector.Y + 1) / 2f;
                        texVector.Z = (normalVector.Z + 1) / 2f;
                        data[x + y * _height] = new Color(texVector);
                    }
                    else
                    {
                        normX = new Vector3(0, 0, 1);
                        normY = new Vector3(0, 0, 1);
                        normalVector = normX + normY;
                        normalVector.Normalize();
                        Vector3 texVector = Vector3.Zero;
                        texVector.X = (normalVector.X + 1) / 2f;
                        texVector.Y = (normalVector.Y + 1) / 2f;
                        texVector.Z = (normalVector.Z + 1) / 2f;
                        data[x + y * _height] = new Color(texVector);
                    }
                }
            }
            result.SetData(data);
            return result;
        }

        /// <summary>
        /// Creates a grayscale texture map for the current content of the noise map.
        /// </summary>
        /// <param name="device">The graphics device to use.</param>
        /// <returns>The created texture map.</returns>
        public Texture2D GetTexture(GraphicsDevice device)
        {
            return GetTexture(device, Gradient.Grayscale);
        }

        /// <summary>
        /// Creates a texture map for the current content of the noise map.
        /// </summary>
        /// <param name="device">The graphics device to use.</param>
        /// <param name="gradient">The gradient to color the texture map with.</param>
        /// <returns>The created texture map.</returns>
        public Texture2D GetTexture(GraphicsDevice device, Gradient gradient)
        {
            return GetTexture(device, ref gradient);
        }

        /// <summary>
        /// Creates a texture map for the current content of the noise map.
        /// </summary>
        /// <param name="device">The graphics device to use.</param>
        /// <param name="gradient">The gradient to color the texture map with.</param>
        /// <returns>The created texture map.</returns>
        public Texture2D GetTexture(GraphicsDevice device, ref Gradient gradient)
        {
            var result = new Texture2D(device, _width, _height);
            var data = new Color[_width * _height];
            int id = 0;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++, id++)
                {
                    float d;
                    if (!float.IsNaN(_borderValue) && (x == 0 || x == _width - 1 || y == 0 || y == _height - 1))
                    {
                        d = _borderValue;
                    }
                    else
                    {
                        d = _data[x, y];
                    }
                    data[id] = gradient[d];
                }
            }
            result.SetData(data);
            return result;
        }

        [System.Xml.Serialization.XmlIgnore]
#if !XBOX360 && !ZUNE
        [NonSerialized]
#endif
        private bool _disposed;

        /// <summary>
        /// Gets a value whether the object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed) { _disposed = Disposing(); }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        /// <returns>True if the object is completely disposed.</returns>
        protected virtual bool Disposing()
        {
            if (_data != null) { _data = null; }
            _width = 0;
            _height = 0;
            return true;
        }
    }
}