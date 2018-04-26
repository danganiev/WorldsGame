using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace LibNoise.Xna
{
    /// <summary>
    /// Provides a color gradient.
    /// </summary>
    public struct Gradient
    {
        private List<KeyValuePair<double, Color>> _data;
        private bool _inverted;

        private static readonly Gradient _empty;
        private static readonly Gradient _terrain;
        private static readonly Gradient _grayscale;

        /// <summary>
        /// Initializes a new instance of Gradient.
        /// </summary>
        static Gradient()
        {
            _terrain._data = new List<KeyValuePair<double, Color>>
            {
                new KeyValuePair<double, Color>(-1.0, new Color(0, 0, 128)),
                new KeyValuePair<double, Color>(-0.2, new Color(32, 64, 128)),
                new KeyValuePair<double, Color>(-0.04, new Color(64, 96, 192)),
                new KeyValuePair<double, Color>(-0.02, new Color(192, 192, 128)),
                new KeyValuePair<double, Color>(0.0, new Color(0, 192, 0)),
                new KeyValuePair<double, Color>(0.25, new Color(192, 192, 0)),
                new KeyValuePair<double, Color>(0.5, new Color(160, 96, 64)),
                new KeyValuePair<double, Color>(0.75, new Color(128, 255, 255)),
                new KeyValuePair<double, Color>(1.0, new Color(255, 255, 255))
            };
            _terrain._inverted = false;

            _grayscale._data = new List<KeyValuePair<double, Color>>
            {
                new KeyValuePair<double, Color>(-1.0, Color.Black),
                new KeyValuePair<double, Color>(1.0, Color.White)
            };

            _grayscale._inverted = false;

            _empty._data = new List<KeyValuePair<double, Color>>
            {
                new KeyValuePair<double, Color>(-1.0, Color.Transparent),
                new KeyValuePair<double, Color>(1.0, Color.Transparent)
            };
            _empty._inverted = false;
        }

        /// <summary>
        /// Initializes a new instance of Gradient.
        /// </summary>
        public Gradient(Color color)
        {
            _data = new List<KeyValuePair<double, Color>>
            {
                new KeyValuePair<double, Color>(-1.0, color),
                new KeyValuePair<double, Color>(1.0, color)
            };

            _inverted = false;
        }

        /// <summary>
        /// Initializes a new instance of Gradient.
        /// </summary>
        public Gradient(Color start, Color end)
        {
            _data = new List<KeyValuePair<double, Color>>
            {
                new KeyValuePair<double, Color>(-1.0, start),
                new KeyValuePair<double, Color>(1.0, end)
            };

            _inverted = false;
        }

        /// <summary>
        /// Gets or sets a gradient step by its position.
        /// </summary>
        /// <param name="position">The position of the gradient step.</param>
        /// <returns>The corresponding color value.</returns>
        public Color this[double position]
        {
            get
            {
                int i;
                for (i = 0; i < _data.Count; i++)
                {
                    if (position < _data[i].Key)
                    {
                        break;
                    }
                }

                var i0 = (int)MathHelper.Clamp(i - 1, 0, _data.Count - 1);
                var i1 = (int)MathHelper.Clamp(i, 0, _data.Count - 1);

                if (i0 == i1)
                {
                    return _data[i1].Value;
                }

                double ip0 = _data[i0].Key;
                double ip1 = _data[i1].Key;
                double a = (position - ip0) / (ip1 - ip0);

                if (_inverted)
                {
                    a = 1.0 - a;
                    //                    double t = ip0;
                    //                    ip0 = ip1;
                    //                    ip1 = t;
                }
                return Color.Lerp(_data[i0].Value, _data[i1].Value, (float)a);
            }
            set
            {
                for (int i = 0; i < _data.Count; i++)
                {
                    if (_data[i].Key == position)
                    {
                        _data.RemoveAt(i);
                        break;
                    }
                }
                _data.Add(new KeyValuePair<double, Color>(position, value));
                _data.Sort((lhs, rhs) => lhs.Key.CompareTo(rhs.Key));
            }
        }

        /// <summary>
        /// Gets or sets a value whether the gradient is inverted.
        /// </summary>
        public bool IsInverted
        {
            get { return _inverted; }
            set { _inverted = value; }
        }

        /// <summary>
        /// Gets the empty instance of Gradient.
        /// </summary>
        public static Gradient Empty
        {
            get { return _empty; }
        }

        /// <summary>
        /// Gets the grayscale instance of Gradient.
        /// </summary>
        public static Gradient Grayscale
        {
            get { return _grayscale; }
        }

        /// <summary>
        /// Gets the terrain instance of Gradient.
        /// </summary>
        public static Gradient Terrain
        {
            get { return _terrain; }
        }

        /// <summary>
        /// Clears the gradient to transparent black.
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            _data.Add(new KeyValuePair<double, Color>(0.0, Color.Transparent));
            _data.Add(new KeyValuePair<double, Color>(1.0, Color.Transparent));
        }

        /// <summary>
        /// Inverts the gradient.
        /// </summary>
        public void Invert()
        {
            // UNDONE: public void Invert()
            throw new NotImplementedException();
        }
    }
}