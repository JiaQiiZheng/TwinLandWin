using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinLand.Components.Instrument.Charactor
{
    public class CharactorObject
    {
        public string Name { get; set; }
        public List<Mesh> Bodies { get; set; }
        public Mesh BodyStatus { get; set; }
        public Point3d BodyLocation { get; set; }
        public double MovingSpeed { get; set; }
        public double TurningSpeed { get; set; }
        public Vector3d Orientation { get; set; }
        public List<int[]> BodyColor { get; set; }
        public int ColorIndex { get; set; }

        public CharactorObject()
        {
            Name = "Olmsted";
            Bodies = new List<Mesh>();
            BodyStatus = Bodies[0];
            BodyLocation = Point3d.Origin;
            MovingSpeed = 100.0;
            TurningSpeed = 1.0;
            Orientation = Vector3d.XAxis;
            ColorIndex = 0;
            BodyColor = colors;
        }

        public CharactorObject(string name, List<Mesh> bodies, double movingSpeed, double turningSpeed, Vector3d orientation)
        {
            Name = name;
            Bodies = bodies;
            MovingSpeed = movingSpeed;
            TurningSpeed = turningSpeed;
            Orientation = orientation;
            ColorIndex = 0;
            BodyColor = colors;
        }

        static int[] color_normal = new int[] { 180, 255, 0, 77 };
        static int[] color_simulation = new int[] { 180, 0, 45, 120 };
        List<int[]> colors = new List<int[]> { color_normal, color_simulation };
    }
}
