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
        public double Mass { set; get; }
        public bool SelfCollision { set; get; }
        public int GroupIndex { set; get; }
        public Point3d Location { set; get; }
        public Vector3d Velocity { set; get; }

        // Constructors
        public SolidParticle()
        {
            Diameter = 10.0;
            Mass = 1.0;
            SelfCollision = true;
            GroupIndex = 0;
            Location = Point3d.Origin;
            Velocity = Vector3d.Zero;
        }

        public SolidParticle(double diameter, double mass, bool selfCollision, int groupIndex)
        {
            Diameter = diameter;
            Mass = mass;
            SelfCollision = selfCollision;
            GroupIndex = groupIndex;
            Location = Point3d.Origin;
            Velocity = Vector3d.Zero;
        }

        // Method
    }
}
