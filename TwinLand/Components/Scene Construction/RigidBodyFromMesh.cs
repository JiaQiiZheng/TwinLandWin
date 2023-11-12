using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Construction
{
    public class RigidBodyFromMesh : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the RigidBodyFromMesh class.
        /// </summary>
        public RigidBodyFromMesh()
          : base("Rigid Body From Mesh", "rigid body from mesh",
              "", "Construction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "Mesh as rigid body", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocity", "velocity", "", GH_ParamAccess.list, new Vector3d(0.0, 0.0, 0.0));
            pManager.AddNumberParameter("Mass", "mass", "", GH_ParamAccess.list, new List<double> { 1.0 });
            pManager.AddNumberParameter("Stiffness", "stiffness", "", GH_ParamAccess.list, new List<double> { 1.0 });
            pManager.AddIntegerParameter("Group Index", "group index", "", GH_ParamAccess.list, new List<int> { 0 });
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Rigid Body", "rigid body", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> meshes = new List<Mesh>();
            List<Vector3d> velocities = new List<Vector3d>();
            List<double> masses = new List<double>();
            List<double> stiffnesses = new List<double>();
            List<int> groupIndices = new List<int>();

            if (!DA.GetDataList("Mesh", meshes)) return;
            DA.GetDataList("Velocity", velocities);
            DA.GetDataList("Mass", masses);
            DA.GetDataList("Stiffness", stiffnesses);
            DA.GetDataList("Group Index", groupIndices);

            List<RigidBody> rigidBodies = new List<RigidBody>();

            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = new Mesh();

                // Simplify meshes
                mesh.Vertices.AddVertices(meshes[i].Vertices);
                mesh.Faces.AddFaces(meshes[i].Faces);

                mesh.UnifyNormals();
                mesh.Normals.ComputeNormals();
                mesh.Normals.UnitizeNormals();

                List<float> vertices = new List<float>();
                List<float> normals = new List<float>();
                List<float> inverseMasses = new List<float>();

                int vertexCount = mesh.Vertices.Count;
                for (int j = 0; j < vertexCount; j++)
                {
                    vertices.Add(mesh.Vertices[j].X);
                    vertices.Add(mesh.Vertices[j].Y);
                    vertices.Add(mesh.Vertices[j].Z);

                    normals.Add(mesh.Normals[j].X);
                    normals.Add(mesh.Normals[j].Y);
                    normals.Add(mesh.Normals[j].Z);

                    double mass = 1.0 / vertexCount;
                    if (masses.Count == 1)
                    {
                        mass = masses[0] / vertexCount;
                    }
                    else if (masses.Count > i)
                    {
                        mass = masses[i] / vertexCount;
                    }
                    inverseMasses.Add((float)(1.0 / Math.Max(mass, 0.00000000001)));
                }

                float[] velocity = new float[3] { 0.0f, 0.0f, 0.0f };
                if (velocities.Count == 1)
                {
                    velocity[0] = (float)velocities[0].X;
                    velocity[1] = (float)velocities[0].Y;
                    velocity[2] = (float)velocities[0].Z;
                }
                else if (velocities.Count > i)
                {
                    velocity[0] = (float)velocities[i].X;
                    velocity[1] = (float)velocities[i].Y;
                    velocity[2] = (float)velocities[i].Z;
                }

                float stiffness = 1.0f;
                if (stiffnesses.Count == 1) stiffness = (float)stiffnesses[0];
                else if (stiffnesses.Count > i) stiffness = (float)stiffnesses[i];

                int groupIndex = i;
                if (groupIndices.Count == 1) groupIndex += groupIndices[0];
                else if (groupIndices.Count > i) groupIndex = groupIndices[i];

                RigidBody rb = new RigidBody(vertices.ToArray(), velocity, normals.ToArray(), inverseMasses.ToArray(), stiffness, groupIndex);
                rb.Mesh = mesh;
                rigidBodies.Add(rb);
            }

            DA.SetDataList("Rigid Body", rigidBodies);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.TL_Engine;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5EAC167C-2D56-4C3F-BBE8-1A54BDF49049"); }
        }
    }
}