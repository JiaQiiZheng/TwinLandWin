using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinLand.Utils
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
            this.Name = "Olmsted";
            this.Bodies = new List<Mesh>();
            this.BodyStatus = Bodies[0];
            this.BodyLocation = Point3d.Origin;
            this.MovingSpeed = 100.0;
            this.TurningSpeed = 1.0;
            this.Orientation = Vector3d.XAxis;
            this.ColorIndex = 0;
            this.BodyColor = colors;
        }

        public CharactorObject(string name, List<Mesh> bodies, double movingSpeed, double turningSpeed, Vector3d orientation)
        {
            this.Name = name;
            this.Bodies = bodies;
            this.MovingSpeed = movingSpeed;
            this.TurningSpeed = turningSpeed;
            this.Orientation = orientation;
            this.ColorIndex = 0;
            this.BodyColor = colors;
        }

        static int[] color_normal = new int[] { 180, 255, 0, 77 };
        static int[] color_simulation = new int[] { 180, 0, 45, 120 };
        List<int[]> colors = new List<int[]> { color_normal, color_simulation };
    }
}
