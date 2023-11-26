using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinLand.Components.Instrument.Materials;

namespace TwinLand.Components.Instrument.Land_Brush
{
    public class LandBrush
    {
        // Properties
        public Point3d Location { get; set; }
        public double Radius { get; set; }
        public Curve Zone { get; set; }
        public Mesh Topography { get; set; }
        public double Tolerence { get; set; }
        public MaterialObject MaterialWrapper { get; set; }
        public List<Point3d> Stroke { get; set; }
        public bool Active { get; set; }

        // Static variables
        int uvMaxCount = 1000;


        // Constructors
        public LandBrush(double radius, Mesh topography, double tl, MaterialObject materialWrapper)
        {
            Radius = radius;
            Topography = topography;
            Tolerence = tl;
            Location = Point3d.Origin;
            MaterialWrapper = materialWrapper;
            Stroke = new List<Point3d>();
        }


        // Methods
        public void UpdateZone(Point3d location)
        {
            this.Location = location;
            Point3d projectedLocation = new Point3d(location);
            projectedLocation.Z = 0;
            Circle circle = new Circle(new Plane(projectedLocation, Vector3d.ZAxis), this.Radius);
            Curve zone = Curve.ProjectToMesh(circle.ToNurbsCurve(), this.Topography, Vector3d.ZAxis, this.Tolerence)[0];

            this.Zone = zone;
        }

        public void UpdateZoneRadius()
        {
            Point3d projectedLocation = new Point3d(this.Location);
            projectedLocation.Z = 0;
            Circle circle = new Circle(new Plane(projectedLocation, Vector3d.ZAxis), this.Radius);
            Curve zone = Curve.ProjectToMesh(circle.ToNurbsCurve(), this.Topography, Vector3d.ZAxis, this.Tolerence)[0];

            // Lift up to avoid overlap with mesh
            zone.Translate(new Vector3d(0, 0, 100));

            this.Zone = zone;
        }

        public void UpdateStroke(double tl)
        {
            List<Point3d> locations = new List<Point3d>();
            Point3d projectedLocation = new Point3d(this.Location);
            projectedLocation.Z = 0;
            Curve bound = new Circle(new Plane(projectedLocation, Vector3d.ZAxis), this.Radius).ToNurbsCurve();
            Brep[] bp = Brep.CreatePlanarBreps(bound, tl);
            Surface srf = bp[0].Faces[0].ToNurbsSurface();

            // Calculate appropriate uv count based on input configurations
            double diameter = 1.0;
            double sparsity = 1.0;

            if (MaterialWrapper.SolidParticle != null)
            {
                diameter = MaterialWrapper.SolidParticle.Diameter;
                sparsity = MaterialWrapper.SolidParticle.Sparsity;
            }


            int uCount = Math.Min(uvMaxCount, (int)(this.Radius * 2 / (diameter * sparsity)));
            int vCount = Math.Min(uvMaxCount, (int)(this.Radius * 2 / (diameter * sparsity)));

            locations = DivideSurfaceIntoGrid(srf, bound, uCount, vCount, true, tl);

            Point3d[] projected = Intersection.ProjectPointsToMeshes(new Mesh[] { this.Topography }, locations, Vector3d.ZAxis, tl);

            // Lift up particles to make sure they do not stuck onto mesh
            List<Point3d> lifted = new List<Point3d>();
            foreach (Point3d pt in projected)
            {
                Point3d newPt = new Point3d(pt.X, pt.Y, pt.Z + diameter);
                lifted.Add(newPt);
            }

            this.Stroke = lifted;
        }

        private List<Point3d> DivideSurfaceIntoGrid(Surface surface, Curve crv, int uCount, int vCount, bool inside, double tl)
        {
            List<Point3d> points = new List<Point3d>();

            if (surface == null || uCount <= 0 || vCount <= 0)
                return points;

            // Reparameterize surface
            surface.SetDomain(0, new Interval(0, 1));
            surface.SetDomain(1, new Interval(0, 1));

            for (int i = 0; i < uCount; i++)
            {
                for (int j = 0; j < vCount; j++)
                {
                    double u = i / (double)(uCount - 1);
                    double v = j / (double)(vCount - 1);

                    Point3d point = surface.PointAt(u, v);

                    // Cull points outside of boundary
                    if (inside)
                    {
                        PointContainment pc = crv.Contains(point, Plane.WorldXY, tl);
                        if (pc != PointContainment.Inside)
                        {
                            continue;
                        }
                    }

                    points.Add(point);
                }
            }

            return points;
        }
    }
}
