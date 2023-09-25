using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinLand
{
    //public void Test()
    //{
    //    Flex flex = new Flex();
    //    flex.Scene.RegisterCustomConstraints()
    //}


    public class ConstraintSystem
    {
        // properties
        public int[] AnchorIndices;
        public int[] ShapeMatchingIndices;
        public float ShapeStiffness;
        public int[] SpringPairIndies;
        public float[] SpringStiffness;
        public float[] SpringDefaultLengths;
        public int[] TriangleIndices;
        public float[] TriangleNormals;

        public int TimeStamp;
    }
}
