using System;
using System.Collections.Generic;
using Eto.Forms;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Construction
{
    public class MeshReduce : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ReduceMesh class.
        /// </summary>
        public MeshReduce()
          : base("Mesh Reduce", "mesh reduce",
              "", "Construction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Desire Face Count", "desire face count", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Accuracy", "accuracy", "Integer from 1 to 10 telling how accurate reduction algorithm to use. Greater number gives more accurate results\r\n", GH_ParamAccess.item, 10);
            pManager.AddBooleanParameter("Normalize Size", "normalize size", "If True mesh is fitted to an axis aligned unit cube until reduction is complete\r\n", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Allow Distortion", "allow distortion", "If True mesh appearance is not changed even if the target polygon count is not reached\r\n", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Weld", "weld", "", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Weld Angle", "weld angle", "", GH_ParamAccess.item, 180.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Reduce Percentage", "reduce percentage", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            int desireFaceCount = 1;
            int accuracy = 10;
            bool normalizeSize = true;
            bool allowDistortion = true;
            bool weld = false;
            double weldAngle = 180.0;

            if (!DA.GetData("Mesh", ref mesh)) return;
            if (!DA.GetData("Desire Face Count", ref desireFaceCount)) return;
            DA.GetData("Accuracy", ref accuracy);
            DA.GetData("Normalize Size", ref normalizeSize);
            DA.GetData("Allow Distortion", ref allowDistortion);
            DA.GetData("Weld", ref weld);
            DA.GetData("Weld Angle", ref weldAngle);

            // Make a shallow copy in case input mesh is too large
            Mesh reducedMesh = new Mesh();
            reducedMesh.Vertices.AddVertices(mesh.Vertices);
            reducedMesh.Faces.AddFaces(mesh.Faces);

            // Weld reducedMesh
            if (weld)
            {
                reducedMesh.Weld(RhinoMath.ToRadians(weldAngle));
            }
            
            // Recompute normals;
            reducedMesh.UnifyNormals();
            reducedMesh.Normals.ComputeNormals();
            reducedMesh.Normals.UnitizeNormals();

            //desireFaceCount = Math.Min(desireFaceCount, mesh.Faces.Count);
            bool result = reducedMesh.Reduce(desireFaceCount, allowDistortion, accuracy, normalizeSize);
            if (!result)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh reduce failed.");
            }

            double rp = (double)desireFaceCount / mesh.Faces.Count;
            string reducePercentage = String.Empty;
            if (rp < 1)
            {
                reducePercentage = $"Mesh reduced to {rp.ToString("f2")} times of face count.";
            }
            else
            {
                reducePercentage = $"Mesh had not reduced.";
            }

            DA.SetData("Mesh", reducedMesh);
            DA.SetData("Reduce Percentage", reducePercentage);
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
                return Properties.Resources.T_icon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("41AF3D33-713B-4F44-9469-0C18E37C067C"); }
        }
    }
}