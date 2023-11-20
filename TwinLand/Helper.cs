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
    }

    public class ZComparer : IComparer<Point3d>
    {
        public int Compare(Point3d p1, Point3d p2)
        {
            return p2.Z.CompareTo(p1.Z);
        }
    }
}