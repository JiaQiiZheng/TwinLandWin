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
        public double MovingSpeed { get; set; }
        public double TurningSpeed { get; set; }

        public CharactorObject()
        {
            this.Name = "Olmsted";
            this.Bodies = new List<Mesh>();
            this.MovingSpeed = 100.0;
            this.TurningSpeed = 1.0;
        }

        public CharactorObject(string name, List<Mesh> bodies, double movingSpeed, double turningSpeed)
        {
            this.Name = name;
            this.Bodies = bodies;
            this.MovingSpeed = movingSpeed;
            this.TurningSpeed = turningSpeed;
        }
    }
}
