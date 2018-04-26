using System;
using Microsoft.Xna.Framework;

namespace WorldsGame.Utils
{
    internal class MathUtils
    {
        //Linear interpolation
        internal static double Lerp(double x, double x1, double x2, double q00, double q01)
        {
            double xNumberedDifference = x2 - x1;
            double firstPart = (x2 - x) / xNumberedDifference;
            double secondPart = (x - x1) / xNumberedDifference;

            double result = firstPart * q00 + secondPart * q01;

            return result;
        }

        //Bilinear interpolation
        internal static double BiLerp(double x, double y, double q11, double q12, double q21, double q22, double x1, double x2, double y1, double y2)
        {
            double r1 = Lerp(x, x1, x2, q11, q21);
            double r2 = Lerp(x, x1, x2, q12, q22);

            return Lerp(y, y1, y2, r1, r2);
        }

        //Trilinear interpolation
        internal static double TriLerp(double x, double y, double z, double q000, double q001, double q010, double q011, double q100, double q101, double q110, double q111, double x1, double x2, double y1, double y2, double z1, double z2)
        {
            double x00 = Lerp(x, x1, x2, q000, q100);
            double x10 = Lerp(x, x1, x2, q010, q110);
            double x01 = Lerp(x, x1, x2, q001, q101);
            double x11 = Lerp(x, x1, x2, q011, q111);
            double r0 = Lerp(y, y1, y2, x00, x01);
            double r1 = Lerp(y, y1, y2, x10, x11);

            return Lerp(z, z1, z2, r0, r1);
        }

        /**
         * Applies Cantor's pairing function to 2D coordinates.
         *
         * @param k1 X-coordinate
         * @param k2 Y-coordinate
         * @return Unique 1D value
         */

        internal static ulong Cantorize2D(ulong k1, ulong k2)
        {
            return ((k1 + k2) * (k1 + k2 + 1) / 2) + k2;
        }

        internal static ulong Cantorize3D(uint k1, uint k2, uint k3)
        {
            return Cantorize2D(Cantorize2D(k1, k2), k3);
        }

        internal static uint Fold(int i)
        {
            if (i >= 0)
            {
                return (uint)(2 * i);
            }

            return (uint)(2 * Math.Abs(i) - 1);
        }

        internal static bool CheckIfParallelToXZ(Vector3 vector)
        {
            Vector3 xzNormal = Vector3.Cross(Vector3.Forward, Vector3.Left);
            xzNormal.Normalize();

            Vector3 vectorNormal = Vector3.Cross(Vector3.Forward, vector);
            vectorNormal.Normalize();

            float dotProduct = Math.Abs(Vector3.Dot(xzNormal, vectorNormal));

            if (dotProduct > 0.999f)
            {
                return true;
            }

            return false;
        }

        // Found these in some minecraft-like game (named ColonyCraft) on github (Java), they might be useful some day
        /*public static void decantorize3(int c, Vec3i output)
        {
            int j = (int)(Math.sqrt(0.25 + 2 * c) - 0.5);
            int z = c - j * (j + 1) / 2;
            int xandy = j - z;
            j = (int)(Math.sqrt(0.25 + 2 * c) - 0.5);
            int y = xandy - j * (j + 1) / 2;
            int x = j - y;
            output.set(x, y, z);
        }

        public static int mapToPositiveAndCantorize3(int x, int y, int z)
        {
            return MathHelper.cantorize3(MathHelper.mapToPositive(x), MathHelper.mapToPositive(y), MathHelper.mapToPositive(z));
        }

        public static void decatorize(int c, Vec2i output)
        {
            int j = (int)(Math.sqrt(0.25 + 2 * c) - 0.5);
            int y = c - j * (j + 1) / 2;
            int x = j - y;
            output.set(x, y);
        }

        public static int cantorX(int c)
        {
            int j = (int)(Math.sqrt(0.25 + 2 * c) - 0.5);
            return j - (c - j * (j + 1) / 2);
        }

        public static int cantorY(int c)
        {
            int j = (int)(Math.sqrt(0.25 + 2 * c) - 0.5);
            return c - j * (j + 1) / 2;
        }
         *
         * Maps any given value to be positive only.
         *
        public static int mapToPositive(int x)
        {
            if (x >= 0)
                return x << 1;

            return -(x << 1) - 1;
        }

        */
    }
}