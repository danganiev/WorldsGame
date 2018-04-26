using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LibNoise.Xna
{
    /// <summary>
    /// Defines a collection of quality modes.
    /// </summary>
    public enum QualityMode
    {
        Low,
        Medium,
        High,
    }

    public enum ModuleBaseType
    {
        Simple,
        ValueStorage
    }

    /// <summary>
    /// Base class for noise modules.
    /// </summary>
    public abstract class ModuleBase : IDisposable
    {
        protected ModuleBase[] modules;

        public Dictionary<string, double[][][]> Values { get; set; }

        public void SetValues(string key, double[][][] values)
        {
            if (_type == ModuleBaseType.ValueStorage)
            {
                Values[key] = values;
            }
            foreach (ModuleBase moduleBase in modules)
            {
                moduleBase.SetValues(key, values);
            }
        }

        private ModuleBaseType _type;

        public ModuleBaseType Type
        {
            get
            {
                foreach (ModuleBase moduleBase in modules)
                {
                    if (moduleBase.Type == ModuleBaseType.ValueStorage)
                    {
                        return ModuleBaseType.ValueStorage;
                    }
                }
                return _type;
            }
            protected set { _type = value; }
        }

        protected List<string> _keys;

        public List<string> Keys
        {
            get
            {
                var keys = new List<string>();
                keys.AddRange(_keys);

                foreach (ModuleBase moduleBase in modules)
                {
                    keys.AddRange(moduleBase.Keys);
                }

                return keys;
            }
        }

        /// <summary>
        /// Initializes a new instance of Helpers.
        /// </summary>
        /// <param name="count">The number of source modules.</param>
        protected ModuleBase(int count)
        {
            if (count >= 0)
            {
                modules = new ModuleBase[count];
            }
            _keys = new List<string>();

            Type = ModuleBaseType.Simple;
        }

        /// <summary>
        /// Gets or sets a source module by index.
        /// </summary>
        /// <param name="index">The index of the source module to aquire.</param>
        /// <returns>The requested source module.</returns>
        public virtual ModuleBase this[int index]
        {
            get
            {
                System.Diagnostics.Debug.Assert(modules != null);
                System.Diagnostics.Debug.Assert(modules.Length > 0);
                if (index < 0 || index >= modules.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (modules[index] == null)
                {
                    throw new ArgumentNullException();
                }
                return modules[index];
            }
            set
            {
                System.Diagnostics.Debug.Assert(modules.Length > 0);
                if (index < 0 || index >= modules.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                modules[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the seed of the ridged-multifractal noise.
        /// </summary>
        public virtual int Seed { get; set; }

        /// <summary>
        /// Gets the number of source modules required by this noise module.
        /// </summary>
        public int SourceModuleCount
        {
            get { return (modules == null) ? 0 : modules.Length; }
        }

        public abstract double GetValue(double x, double y, double z);

        /// <summary>
        /// Returns the output value for the given input coordinates.
        /// </summary>
        /// <param name="coordinate">The input coordinate.</param>
        /// <returns>The resulting output value.</returns>
        public double GetValue(Vector3 coordinate)
        {
            return GetValue(coordinate.X, coordinate.Y, coordinate.Z);
        }

        /// <summary>
        /// Returns the output value for the given input coordinates.
        /// </summary>
        /// <param name="coordinate">The input coordinate.</param>
        /// <returns>The resulting output value.</returns>
        public double GetValue(ref Vector3 coordinate)
        {
            return GetValue(coordinate.X, coordinate.Y, coordinate.Z);
        }

        /// <summary>
        /// Sets seed into every module in the module tree
        /// </summary>
        /// <param name="seed">Seed</param>
        /// <returns>Increased seed by count</returns>
        public int SetSeed(int seed)
        {
            Seed = seed;
            int nextSeed = seed + 1;

            if (modules != null)
            {
                foreach (ModuleBase moduleBase in modules)
                {
                    nextSeed = moduleBase.SetSeed(nextSeed);
                }
            }

            return nextSeed;
        }

        /// <summary>
        /// Clears any state data
        /// </summary>
        public void Clear()
        {
            DoClear();
            foreach (ModuleBase moduleBase in modules)
            {
                moduleBase.Clear();
            }
        }

        /// <summary>
        /// Does the actual cleaning
        /// </summary>
        protected virtual void DoClear()
        {
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
            if (modules != null)
            {
                for (int i = 0; i < modules.Length; i++)
                {
                    modules[i].Dispose();
                    modules[i] = null;
                }
                modules = null;
            }
            return true;
        }
    }
}