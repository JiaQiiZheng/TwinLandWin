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
        public float[] SpringTargetLengths;
        public float[] SpringStiffness;
        public float[] SpringDefaultLengths;
        public int[] TriangleIndices;
        public float[] TriangleNormals;

        public int TimeStamp;

        // constructors
        public ConstraintSystem()
        {
            AnchorIndices = new int[0];
            ShapeMatchingIndices = new int[0];
            ShapeStiffness = 0.0f;
            SpringPairIndies = new int[0];
            SpringTargetLengths = new float[0];
            SpringStiffness = new float[0];
            TriangleIndices = new int[0];
            TriangleNormals = new float[0];

            TimeStamp = 0;
        }
        
        public ConstraintSystem(int[] anchorIndices)
        {
            AnchorIndices = anchorIndices;
            ShapeMatchingIndices = ShapeMatchingIndices = new int[0];
            ShapeStiffness = 0.0f;
            SpringPairIndies = new int[0];
            SpringTargetLengths = new float[0];
            SpringStiffness = new float[0];
            TriangleIndices = new int[0];
            TriangleNormals = new float[0];

            TimeStamp = ConvertParams.GetTimeStampInMillisecond();
        }
        
        public ConstraintSystem(int[] shapeMatchingIndices, float shapeStiffness)
        {
            AnchorIndices = new int[0];
            ShapeMatchingIndices = shapeMatchingIndices;
            ShapeStiffness = shapeStiffness;
            SpringPairIndies = new int[0];
            SpringTargetLengths = new float[0];
            SpringStiffness = new float[0];
            TriangleIndices = new int[0];
            TriangleNormals = new float[0];

            TimeStamp = ConvertParams.GetTimeStampInMillisecond();
        }
    }

    class ConvertParams
    {
        public static List<int> GetMemoryRequirementsByPowers(List<int> mr_powers)
        {
            List<int> memoeryRequirements = new List<int>();
            foreach (int power in memoeryRequirements)
            {
                memoeryRequirements.Add((int)Math.Pow(2, power));
            }
            return memoeryRequirements;
        }

        public static int GetTimeStampInMillisecond()
        {
            return DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
        }
    }
}
