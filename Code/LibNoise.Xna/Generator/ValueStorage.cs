using System;
using System.Collections.Generic;

using WorldsLib;

namespace LibNoise.Xna.Generator
{
    public class ValueStorage : ModuleBase
    {
        private string _key;

        public string Key
        {
            get { return _key; }
            private set
            {
                _keys.Clear();
                _key = value;
                _keys.Add(_key);
            }
        }

        private double MinX { get; set; }

        private double MinY { get; set; }

        private double MinZ { get; set; }

        public ValueStorage(string key)
            : base(0)
        {
            Key = key;
            Type = ModuleBaseType.ValueStorage;
            Values = new Dictionary<string, double[][][]>();
        }

        public override double GetValue(double x, double y, double z)
        {
            int iz;
            int iy;
            int ix;
            LocalizePosition(x, y, z, out ix, out iy, out iz);

            if (Values != null)
            {
                try
                {
                    return Values[Key][ix][iz][iy];
                }
                catch (IndexOutOfRangeException)
                {
                    return 0;
                }
            }

            return 0;
        }

        private void LocalizePosition(double x, double y, double z, out int ix, out int iy, out int iz)
        {
            MinX = Math.Min(MinX, x);
            MinY = Math.Min(MinY, y);
            MinZ = Math.Min(MinZ, z);

            Vector3i localPosition = ChunkHelper.GetLocalPosition(new Vector3i((int)x, (int)y, (int)z));

            ix = x >= MinX + ChunkHelper.SIZE.X ? ChunkHelper.SIZE.X : localPosition.X;
            iy = y >= MinY + ChunkHelper.SIZE.Y ? ChunkHelper.SIZE.Y : localPosition.Y;
            iz = z >= MinZ + ChunkHelper.SIZE.Z ? ChunkHelper.SIZE.Z : localPosition.Z;
        }

        protected override void DoClear()
        {
            MinX = double.MaxValue;
            MinY = double.MaxValue;
            MinZ = double.MaxValue;
        }
    }
}