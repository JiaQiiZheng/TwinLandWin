using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinLand.Components.Instrument.Materials
{
    public class SolidParticle
    {
        // Properties
        public double Diameter { set; get; }
        public double Sparsity { set; get; }
        public double Mass { set; get; }
        public Point3d Location { set; get; }
        public Vector3d Velocity { set; get; }

        // Constructors
        public SolidParticle()
        {
            Diameter = 10.0;
            Sparsity = 10.0;
            Mass = 1.0;
            Location = Point3d.Origin;
            Velocity = Vector3d.Zero;
        }

        public SolidParticle(double diameter, double sparsity, double mass)
        {
            // Make sure particles do not get overlap
            sparsity = Math.Max(1.0, sparsity);

            Diameter = diameter;
            Sparsity= sparsity;
            Mass = mass;
            Location = Point3d.Origin;
            Velocity = Vector3d.Zero;
        }

        // Method
    }
}
