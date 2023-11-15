using System;
using System.Collections.Generic;
using MIConvexHull;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using Rhino;
using System.Diagnostics;

namespace TwinLand.Components.Scene_Construction
{
    public class MeshConvexHull : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the MeshConvexHull class.
        /// </summary>
        public MeshConvexHull()
          : base("Mesh Convex Hull", "mesh convex hull",
              "Resample mesh vertices based on distance and create convex hull", "Construction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Minimum Distance", "minimum distance", "", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Weld", "weld", "", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Weld Angle", "weld angle", "", GH_ParamAccess.item, 180.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            double minDistance = 1.0;
            bool weld = true;
            double weldAngle = 180.0;

            if (!DA.GetData("Mesh", ref mesh)) return;
            DA.GetData("Minimum Distance", ref minDistance);
            DA.GetData("Weld", ref weld);
            DA.GetData("Weld Angle", ref weldAngle);

            // Cull duplicate vertices based on distance
            int vertexCount = mesh.Vertices.Count;
            Point3d[] pts = new Point3d[vertexCount]; 
            for (int i = 0; i < vertexCount; i++)
            {
                pts[i] = mesh.Vertices[i];
            }
            Point3d[] culled = Point3d.CullDuplicates(pts, minDistance);

            // Create ConvexHull based on culled pts
            IList<double[]> pointsCoorinates = new List<double[]>();
            foreach (Point3d vertex in culled)
            {
                pointsCoorinates.Add(new double[3] { vertex.X, vertex.Y, vertex.Z });
            }

            var hull = ConvexHull.Create(pointsCoorinates, 0.0001);

            // Convert hull to rhino mesh
            Mesh hullMesh = new Mesh();
            int vertexIndex = 0;
            
            if (hull.Result == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Convex Hull computing error");
                return;
            }

            foreach (var face in hull.Result.Faces)
            {
                for (int j = 0; j < 3; j++)
                {
                    double[] xyz = face.Vertices[j].Position;
                    Point3d pt = new Point3d(xyz[0], xyz[1], xyz[2]);
                    hullMesh.Vertices.Add(pt);
                }
                hullMesh.Faces.AddFace(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                vertexIndex += 3;
            }

            // Weld hull mesh and compute normals
            if (weld)
            {
                hullMesh.Weld(RhinoMath.ToRadians(weldAngle));
            }
            hullMesh.Normals.ComputeNormals();

            // Output mesh
            DA.SetData("Mesh", hullMesh);
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
            get { return new Guid("47B7074E-3E43-498F-ACDA-80F3A5335627"); }
        }
    }
}