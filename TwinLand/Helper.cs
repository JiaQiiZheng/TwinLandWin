using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TwinLand
{
    public class Helper
    {
        public static bool isWindows
        {
            get
            {
                bool res = !(Environment.OSVersion.Platform == PlatformID.Unix ||
                             Environment.OSVersion.Platform == PlatformID.MacOSX);
                return res;
            }
        }

        public static int Adjust(int value, int min, int max)
        {
            value = Math.Max(1, value);
            value = Math.Min(10, value);

            return value;
        }
        
        public static void UnifyCLosedCurveOrientation(Curve curve, bool isClockwise)
        {
            if (isClockwise)
            {
                if (curve.ClosedCurveOrientation(Plane.WorldXY) != CurveOrientation.Clockwise)
                {
                    curve.Reverse();
                }
            }
            else
            {
                if (curve.ClosedCurveOrientation(Plane.WorldXY) == CurveOrientation.Clockwise)
                {
                    curve.Reverse();
                }
            }
        }
    }

    public class ZComparer : IComparer<Point3d>
    {
        public int Compare(Point3d p1, Point3d p2)
        {
            return p2.Z.CompareTo(p1.Z);
        }
    }
}