using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface.Controls.Desktop;

namespace WorldsGame.Utils.ExtensionMethods
{
    internal static class ListExtensions
    {
        internal static readonly Random RANDOM = new Random();

        internal static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = RANDOM.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    internal static class ListControlExtensions
    {
        internal static string SelectedName(this ListControl listControl)
        {
            return listControl.SelectedItems.Count > 0 ? listControl.Items[listControl.SelectedItems[0]] : null;
        }

        internal static bool IsSelected(this ListControl listControl)
        {
            return listControl.SelectedItems.Count != 0;
        }
    }

    internal static class Texture2DExtensions
    {
        internal static Texture2D Crop(this Texture2D image, Rectangle source)
        {
            var graphics = image.GraphicsDevice;
            var ret = new RenderTarget2D(graphics, source.Width, source.Height);
            var sb = new SpriteBatch(graphics);

            graphics.SetRenderTarget(ret); // draw to image
            graphics.Clear(new Color(0, 0, 0, 0));

            sb.Begin();
            sb.Draw(image, Vector2.Zero, source, Color.White);
            sb.End();

            graphics.SetRenderTarget(null); // set back to main window

            return (Texture2D)ret;
        }
    }

    internal static class BoundingBoxExtensions
    {
        internal static BoundingBox GetBoundingBox(this BoundingBox box, Vector3 position, float yRotation)
        {
            var newMin = Vector3.Transform(box.Min, Matrix.CreateRotationY(yRotation));
            var newMax = Vector3.Transform(box.Max, Matrix.CreateRotationY(yRotation));

            var boundingBox = new BoundingBox(newMin + position, newMax + position);

            return boundingBox;
        }
    }

    internal static class EnumUtils
    {
        internal static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}