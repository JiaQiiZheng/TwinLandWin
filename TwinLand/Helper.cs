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

    public class MeshInfo
    {
        public static int[] GetMeshUVCount(Mesh mesh)
        {
            // Get Mesh domain
            BoundingBox bb = mesh.GetBoundingBox(true);
            double x_domain = Math.Abs(bb.Max.X - bb.Min.X);
            double y_domain = Math.Abs(bb.Max.Y - bb.Min.Y);

            // Get Mesh V interval and count
            double v_interval = Math.Abs(mesh.Vertices[1].Y - mesh.Vertices[0].Y);
            int v_count = (int)(Math.Ceiling(y_domain / v_interval));

            // Get Mesh U interval and count
            double u_interval = Math.Abs(mesh.Vertices[v_count + 1].X - mesh.Vertices[0].X);
            int u_count = (int)(Math.Ceiling(x_domain / u_interval));

            return new int[2] { u_count, v_count };
        }

        public static double[] GetMeshInterval(Mesh mesh)
        {
            // Get Mesh domain
            BoundingBox bb = mesh.GetBoundingBox(true);
            double x_domain = Math.Abs(bb.Max.X - bb.Min.X);
            double y_domain = Math.Abs(bb.Max.Y - bb.Min.Y);

            // Get Mesh V interval and count
            double v_interval = Math.Abs(mesh.Vertices[1].Y - mesh.Vertices[0].Y);
            int v_count = (int)(Math.Ceiling(y_domain / v_interval));

            // Get Mesh U interval and count
            double u_interval = Math.Abs(mesh.Vertices[v_count + 1].X - mesh.Vertices[0].X);
            int u_count = (int)(Math.Ceiling(x_domain / u_interval));

            return new double[] { u_interval, v_interval };
        }

        public static double[] GetMeshDomain(Mesh mesh)
        {
            // Get Mesh domain
            BoundingBox bb = mesh.GetBoundingBox(true);
            double x_domain = Math.Abs(bb.Max.X - bb.Min.X);
            double y_domain = Math.Abs(bb.Max.Y - bb.Min.Y);

            return new double[] { x_domain, y_domain };
        }

        public static double[] GetMeshInfo(Mesh mesh)
        {
            // Get Mesh domain
            BoundingBox bb = mesh.GetBoundingBox(true);
            double x_domain = Math.Abs(bb.Max.X - bb.Min.X);
            double y_domain = Math.Abs(bb.Max.Y - bb.Min.Y);

            // Get Mesh V interval and count
            double v_interval = Math.Abs(mesh.Vertices[1].Y - mesh.Vertices[0].Y);
            int v_count = (int)(Math.Ceiling(y_domain / v_interval));

            // Get Mesh U interval and count
            double u_interval = Math.Abs(mesh.Vertices[v_count + 1].X - mesh.Vertices[0].X);
            int u_count = (int)(Math.Ceiling(x_domain / u_interval));

            return new double[] { x_domain, y_domain, (double)u_count, (double)v_count, u_interval, v_interval };
        }
    }
}