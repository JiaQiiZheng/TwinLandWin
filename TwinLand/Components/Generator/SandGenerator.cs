using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Display;
using Rhino.Geometry;
using MIConvexHull;
using Rhino.DocObjects.SnapShots;
using System.Linq;
using Rhino;
using System.Runtime.CompilerServices;
using Grasshopper.Kernel.Types.Transforms;

namespace TwinLand.Components.Generator
{
    public class SandGenerator : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the SandGenerator class.
        /// </summary>
        public SandGenerator()
          : base("Sand Generator", "sand generator",
              "", "Generator")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "plane", "", GH_ParamAccess.list, Plane.WorldXY);
            pManager.AddNumberParameter("Maximum Width", "max width", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Maximum Length", "max length", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Maximum Height", "max height", "", GH_ParamAccess.item);

            pManager.AddIntegerParameter("Vertex Count", "vertex count", "", GH_ParamAccess.item, 6);
            pManager.AddNumberParameter("Size Even Factor", "size even factor", "", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.tree);
            pManager.AddPointParameter("Vertices", "vertices", "", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Constraint Point Indices", "constraint point indices", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Plane> planes = new List<GH_Plane>();
            double maxWidth = 1.0;
            double maxLength = 1.0;
            double maxHeight = 1.0;
            int vertexCount = 6;
            double sizeEvenFactor = 1.0;

            if (!DA.GetDataList("Plane", planes)) return;

            if (!DA.GetData("Maximum Width", ref maxWidth)) return;
            if (!DA.GetData("Maximum Length", ref maxLength)) return;
            if (!DA.GetData("Maximum Height", ref maxHeight)) return;
            if (maxWidth <= 0 || maxLength <= 0 || maxHeight <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid dimension to construct 3D geometry.");
            }

            DA.GetData("Vertex Count", ref vertexCount);
            if (vertexCount < 4)
            {
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Vertex Count needs to be larger than 4 to form a mesh.");
                    return;
                }
            }

            DA.GetData("Size Even Factor", ref sizeEvenFactor);
            if (sizeEvenFactor < 0.0 || sizeEvenFactor > 1.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Size Even Factor needs to be value between 0.0 and 1.0");
                return;
            }

            // declare tree to contains particle mesh and vertices of each
            GH_Structure<GH_Point> vertices = new GH_Structure<GH_Point>();
            GH_Structure<GH_Mesh> hullMeshTree = new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Integer> constraintPointIndices = new GH_Structure<GH_Integer>();
            int constraintPointIndex = -1;

            // generate points based on box domain
            double sizeFactor = sizeEvenFactor + rd.NextDouble() * (1 - sizeEvenFactor);
            
            for (int i = 0; i < planes.Count; i++)
            {
                IList<double[]> pointsCoordinates = new List<double[]>();
                Plane plane = planes[i].Value;
                GH_Path groupPath = new GH_Path(i);

                for (int j = 0; j < vertexCount; j++)
                {
                    double x_step = (rd.NextDouble() - 0.5) * maxWidth * sizeFactor;
                    double y_step = (rd.NextDouble() - 0.5) * maxLength * sizeFactor;
                    double z_step = (rd.NextDouble() - 0.5) * maxHeight * sizeFactor;

                    Vector3d move = plane.XAxis * x_step + plane.YAxis * y_step + plane.ZAxis * z_step;

                    Point3d pt = plane.Origin + move;

                    double x = pt.X;
                    double y = pt.Y;
                    double z = pt.Z;

                    double[] xyz = { x, y, z };
                    GH_Point gh_pt = new GH_Point(pt);

                    vertices.Append(gh_pt, groupPath);
                    pointsCoordinates.Add(xyz);

                    // count constraint point index and append into output constraint point indices tree
                    constraintPointIndex += 1;
                    constraintPointIndices.Append(new GH_Integer(constraintPointIndex), groupPath);
                }

                // create 3d convex hull
                var hull = ConvexHull.Create(pointsCoordinates, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                // convert hull to rhino mesh
                var hullMesh = new Mesh();
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
                hullMesh.Weld(RhinoMath.ToRadians(180));
                hullMesh.Normals.ComputeNormals();

                // Append Mesh into hullMeshTree
                hullMeshTree.Append(new GH_Mesh(hullMesh), groupPath);
            }

            DA.SetDataTree(0, hullMeshTree);
            DA.SetDataTree(1, vertices);
            DA.SetDataTree(2, constraintPointIndices);
        }


        /// <summary>
        /// Dynamic variables
        /// </summary>
        Random rd = new Random();

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
            get { return new Guid("E60BFDFB-B358-4818-AACE-9AB4EF637440"); }
        }
    }
}