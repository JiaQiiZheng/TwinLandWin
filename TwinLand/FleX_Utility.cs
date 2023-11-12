using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinLand
{
    // FLuids
    class Fluid
    {
        public float[] Positions { get; private set; }
        public float[] Velocities { get; private set; }
        public float[] InverseMasses { get; private set; }
        public int GroupIndex { get; private set; }

        public Fluid(float[] positions, float[] velocities, float[] inverseMasses, int groupIndex)
        {
            this.Positions = positions;
            this.Velocities = velocities;
            this.InverseMasses = inverseMasses;
            this.GroupIndex = groupIndex;
        }

        public override string ToString()
        {
            string str = "Fluid Group: ";
            str += $"\nParticle Count: {this.Positions.Length / 3}";
            str += $"\nGroup Index: {this.GroupIndex}";
            return str + "\n";
        }
    }

    // Rigid Body
    class RigidBody
    {
        public float[] Vertices;
        public float[] Velocity;
        public float[] VertexNormals;
        public float[] InverseMasses;
        public float Stiffness;
        public int GroupIndex;
        public float[] MassCenter;
        public Mesh Mesh;

        public RigidBody(float[] vertices, float[] velocity, float[] vertexNormals, float[] inverseMass, float stiffness, int groupIndex)
        {
            this.Vertices = vertices;
            this.Velocity = velocity;
            this.VertexNormals = vertexNormals;
            this.InverseMasses = inverseMass;
            this.Stiffness = stiffness;
            this.GroupIndex = groupIndex;

            this.MassCenter = new float[3] { 0.0f, 0.0f, 0.0f };
            int round = vertices.Length / 3;
            for (int i = 0; i < round; i++)
            {
                MassCenter[0] += vertices[3 * i];
                MassCenter[1] += vertices[3 * i + 1];
                MassCenter[2] += vertices[3 * i + 2];
            }

            for (int i = 0; i < 3; i++)
            {
                MassCenter[i] /= round;
            }
        }

        public bool HasMesh()
        {
            return Mesh != null && Mesh.IsValid;
        }

        public override string ToString()
        {
            string str = "Rigid Body:";
            str += $"\nVertex Count: {Vertices.Length / 3}";
            str += $"\nTotal Mass: {(Vertices.Length / 3 * (1.0 / InverseMasses[0]))}";
            str += $"\nStiffness: {Stiffness}";
            str += $"\nGroup Index: {GroupIndex}";
            return str + "\n";
        }
    }

    // Cosntraint System
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
