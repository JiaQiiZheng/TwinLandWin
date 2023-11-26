using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinLand.Utils
{
    public class TwinLandParticle
    {
        // Properties
        public double MaximumWidth { get; set; }
        public double MaximumLength { get; set; }
        public double MaximumHeight { get; set; }

        public double Width { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }

        public Plane Plane { get; set; }
        public int VertexCount { get; }
        public Point3d[] Vertices { get; }
        public double SizeEvenFactor { get; set; }

        // Constructors
        public TwinLandParticle()
        {
            this.MaximumWidth = 1.0;
            this.MaximumLength = 1.0;
            this.MaximumHeight = 1.0;
            this.VertexCount = 12;
            this.SizeEvenFactor = 1.0;
        }

        public TwinLandParticle(double maxWidth, double maxLength, double maxHeight, int vertextCount, double sizeEvenFactor)
        {
            this.MaximumWidth = maxWidth;
            this.MaximumLength = maxLength;
            this.MaximumHeight = maxHeight;

            this.VertexCount = vertextCount;
            this.SizeEvenFactor = sizeEvenFactor;

            // Implement particle parameters to generate an instance
        }
    }
}
