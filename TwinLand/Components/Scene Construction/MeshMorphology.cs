using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types.Transforms;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Construction
{
    public class MeshMorphology : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the MeshMorphology class.
        /// </summary>
        public MeshMorphology()
          : base("Mesh Morphology", "mesh morphology",
              "Description", "Construction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Deposition Points", "deposition points", "", GH_ParamAccess.list);
            pManager.AddPointParameter("Erosion Points", "erosion points", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Update Erosion Points", "update erosion points", "", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Morphology Factor", "morphology factor", "", GH_ParamAccess.item, 100);
            pManager.AddBooleanParameter("Reset", "reset", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Active", "active", "", GH_ParamAccess.item, true);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
            pManager.AddMeshFaceParameter("Face", "face", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool active = false;
            DA.GetData("Active", ref active);
            if (!active) return;


            Mesh mesh = null;
            List<Point3d> pts_deposition = new List<Point3d>();
            List<Point3d> pts_erosion = new List<Point3d>();
            bool updateInternalPt = false;
            double mf = 100.0;

            if (!DA.GetData("Mesh", ref mesh)) return;
            DA.GetDataList("Deposition Points", pts_deposition);
            DA.GetDataList("Erosion Points", pts_erosion);
            DA.GetData("Update Erosion Points", ref updateInternalPt);
            DA.GetData("Morphology Factor", ref mf);

            bool reset = false;
            DA.GetData("Reset", ref reset);
            if (reset)
            {
                erosionDepth = 0;
            }

            double[] meshInfo = TwinLand.MeshInfo.GetMeshInfo(mesh);
            double x_domain = meshInfo[0];
            double y_domain = meshInfo[1];
            int u_count = (int)meshInfo[2];
            int v_count = (int)meshInfo[3];
            double u_interval = meshInfo[4];
            double v_interval = meshInfo[5];

            BoundingBox bb = mesh.GetBoundingBox(true);

            List<MeshFace> faces = new List<MeshFace>();
            Mesh morphMesh = new Mesh();
            morphMesh.CopyFrom(mesh);

            double[] morphValues = new double[morphMesh.Vertices.Count];
            Point3d originVertex = mesh.Vertices[0];

            // Keep increase erosion depth every time when update erosion points
            if (updateInternalPt)
            {
                erosionDepth += mf;
            }

            if (pts_erosion != null && pts_erosion.Count > 0)
            {
                foreach (Point3d pt in pts_erosion)
                {
                    // Filter pt out of mesh
                    if (pt.X < bb.Min.X || pt.X > bb.Max.X || pt.Y < bb.Min.Y || pt.Y > bb.Max.Y)
                    {
                        continue;
                    }

                    int u_index = (int)Math.Floor(Math.Abs(pt.X - originVertex.X) / u_interval);
                    int v_index = (int)Math.Floor(Math.Abs(pt.Y - originVertex.Y) / v_interval);

                    var face = mesh.Faces[u_index * v_count + v_index];
                    faces.Add(face);

                    // Add morph value into the collection
                    morphValues[face.A] -= erosionDepth;
                    morphValues[face.B] -= erosionDepth;
                    morphValues[face.C] -= erosionDepth;
                    morphValues[face.D] -= erosionDepth;
                }
            }

            // Start to compute deposition
            foreach (Point3d pt in pts_deposition)
            {
                // Filter pt out of mesh
                if (pt.X <= bb.Min.X || pt.X >= bb.Max.X || pt.Y <= bb.Min.Y || pt.Y >= bb.Max.Y)
                {
                    continue;
                }

                int u_index = (int)Math.Floor(Math.Abs(pt.X - originVertex.X) / u_interval);
                int v_index = (int)Math.Floor(Math.Abs(pt.Y - originVertex.Y) / v_interval);

                var face = mesh.Faces[u_index * v_count + v_index];
                faces.Add(face);

                // Add morph value into the collection
                morphValues[face.A] += mf;
                morphValues[face.B] += mf;
                morphValues[face.C] += mf;
                morphValues[face.D] += mf;
            }

            // Apply morph values on mesh
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                double mv = morphValues[i];
                if (mv == 0) continue;

                morphMesh.Vertices[i] = new Point3f(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z + (float)mv);
            }

            morphMesh.Normals.ComputeNormals();

            DA.SetData("Mesh", morphMesh);
            DA.SetDataList("Face", faces);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        double erosionDepth = 0;

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
            get { return new Guid("F7371763-1835-4B55-8E28-0A54EAE163AB"); }
        }
    }
}