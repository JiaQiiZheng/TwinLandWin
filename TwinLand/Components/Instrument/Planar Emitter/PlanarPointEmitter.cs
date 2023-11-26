using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.PlugIns;
using Rhino.Render.ChangeQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Timers;

namespace TwinLand.Components.Instrument.Planar_Emitter
{
    public class PlanarPointEmitter
    {
        public Plane Plane { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int Count { get; set; }
        public double RandomFactor { get; set; }
        public double Velocity { get; set; }
        public bool IsRandom { get; set; }
        public bool Active { get; set; }
        public Rectangle3d EmittingBoundary { get; set; }
        public List<Point3d> Points { get; set; }
        public List<Vector3d> Velocities { get; set; }

        Random rd = new Random();

        public PlanarPointEmitter()
        {
            this.Plane = Plane.WorldXY;
            this.Width = 1.0;
            this.Height = 1.0;
            this.Count = 0;
            this.RandomFactor = 0.0;
            this.Velocity = 1.0;
            this.IsRandom = true;
            this.EmittingBoundary = new Rectangle3d();
            this.Active = false;
            this.Points = new List<Point3d>();
            this.Velocities = new List<Vector3d>();
        }

        public PlanarPointEmitter(Plane plane, double width, double height, int count, double randomFactor, double velocity, bool isRandom, Rectangle3d emittingBoundary)
        {
            Plane = plane;
            Width = width;
            Height = height;
            Count = count;
            RandomFactor = randomFactor;
            Velocity = velocity;
            IsRandom = isRandom;
            EmittingBoundary = emittingBoundary;
            this.Active = false;
            Points = new List<Point3d>();
            Velocities = new List<Vector3d>();
        }

        // Methods
        public void Trigger(Plane plane)
        {
            if (Count <= 0)
            {
                return;
            }

            // Generate points and velocities
            GeneratePoints(plane, this.Width, this.Height, this.Count, this.RandomFactor, this.Velocity, this.IsRandom, this.EmittingBoundary);
        }

        private void GeneratePoints(Plane plane, double width, double height, int count, double randomFactor, double velocity, bool isRandom, Rectangle3d emittingBoundary)
        {
            // generate emitting boundary
            emittingBoundary = new Rectangle3d(plane, new Interval(-width / 2, width / 2), new Interval(-height / 2, height / 2));
            this.EmittingBoundary = emittingBoundary;

            List<Point3d> pts = new List<Point3d>();
            List<Vector3d> vls = new List<Vector3d>();

            if (isRandom)
            {
                for (int i = 0; i < count; i++)
                {
                    // random in circle area
                    //pts.Add(new Point3d(plane.PointAt((rd.NextDouble() - 0.5) * radius, (rd.NextDouble() - 0.5) * radius)));

                    // random in rectangle area
                    pts.Add(new Point3d(plane.PointAt((rd.NextDouble() - 0.5) * width, (rd.NextDouble() - 0.5) * height)));

                    Vector3d vv = plane.ZAxis;
                    vv.Unitize();
                    vv *= velocity;
                    Vector3d rotA = plane.XAxis;

                    // rotate initial velocity based on random factor
                    rotA.Rotate(rd.NextDouble() * Math.PI * 2, plane.ZAxis);

                    vv.Rotate((rd.NextDouble() - 0.5) * 2 * randomFactor, rotA);

                    vls.Add(vv);
                }
            }

            else
            {
                // grid pattern based on input count as the row for emitter
                List<double> param_x = new List<double>();
                List<double> param_y = new List<double>();

                double step = 1.0 / count;
                for (int i = 0; i <= count; i++)
                {
                    param_x.Add(-0.5 + i * step);
                }

                double count_y = height / width * count;
                double step_y = width / height * step;
                for (int i = 0; i <= count_y; i++)
                {
                    param_y.Add(-0.5 + i * step_y);
                }

                // cross-reference
                foreach (double x in param_x)
                {
                    foreach (double y in param_y)
                    {
                        pts.Add(new Point3d(plane.PointAt(x * width, y * height)));
                    }
                }

                Vector3d vv = plane.ZAxis;
                vv.Unitize();
                vv *= velocity;

                // rotate initial velocity based on random factor
                for (int j = 0; j < pts.Count; j++)
                {
                    Vector3d rotA = plane.XAxis;
                    rotA.Rotate(rd.NextDouble() * Math.PI * 2, plane.ZAxis);
                    vv.Rotate((rd.NextDouble() - 0.5) * Math.PI * 2 * randomFactor, rotA);
                    vls.Add(vv);
                }
            }

            this.Points = pts;
            this.Velocities = vls;
        }
    }
}
