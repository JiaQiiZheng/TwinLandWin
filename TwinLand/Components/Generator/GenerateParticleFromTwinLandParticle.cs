using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MIConvexHull;
using Rhino;
using Rhino.Geometry;
using TwinLand.Utils;

namespace TwinLand.Components.Generator
{
    public class GenerateParticleFromTwinLandParticle : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the TwinLandParticleGenerator class.
        /// </summary>
        public GenerateParticleFromTwinLandParticle()
          : base("Generate Particle from TwinLand Particle", "particle from TL_particle",
              "Generate particles from TwinLand Particle Class", "Generator")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Planes", "planes", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("TwinLand Particle", "TL particle", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Simulation Mode", "simulation mode", "", GH_ParamAccess.item, true);
            //pManager.AddBooleanParameter("Recording Mode", "recording mode", "", GH_ParamAccess.item, true);
            //pManager.AddBooleanParameter("Birth Order", "birth order", "Output meshes based on birth order of the particle", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Reset", "reset", "", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
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
            TwinLandParticle TL_particle = null;
            bool simulationMode = true;
            //bool recordingMode = true;
            //bool birthOrder = true;
            bool reset = false;

            if (!DA.GetDataList("Planes", planes)) return;
            if (!DA.GetData("TwinLand Particle", ref TL_particle)) return;
            DA.GetData("Simulation Mode", ref simulationMode);
            //DA.GetData("Recording Mode", ref recordingMode);
            //DA.GetData("Birth Order", ref birthOrder);
            DA.GetData("Reset", ref reset);

            GH_Structure<GH_Mesh> hullMeshTree = new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Point> vertices = new GH_Structure<GH_Point>();
            GH_Structure<GH_Integer> constraintPointIndices = new GH_Structure<GH_Integer>();
            int constraintPointIndex = -1;


            if (reset)
            {
                dataTree.Clear();
                flipped.Clear();
                DA.SetDataTree(0, dataTree);
                vertices.Append(new GH_Point(), new GH_Path(0));
                DA.SetDataTree(1, vertices);
                DA.SetDataTree(2, constraintPointIndices);
                return;
            }

            int vertexCount = TL_particle.VertexCount;
            double maxWidth = TL_particle.MaximumWidth;
            double maxLength = TL_particle.MaximumLength;
            double maxHeight = TL_particle.MaximumHeight;
            double sizeEvenFactor = TL_particle.SizeEvenFactor;


            for (int i = 0; i < planes.Count; i++)
            {
                IList<double[]> pointsCoordinates = new List<double[]>();
                Plane plane = planes[i].Value;
                GH_Path groupPath = new GH_Path(i);

                double sizeFactor = sizeEvenFactor + rd.NextDouble() * (1 - sizeEvenFactor);
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

                // Record hull meshes into dataTree
                if (simulationMode)
                {
                    dataTree.Append(new GH_Mesh(hullMesh), groupPath);
                }
                else
                {
                    // Append Mesh into hullMeshTree
                    hullMeshTree.Append(new GH_Mesh(hullMesh), groupPath);
                }
            }

            if (simulationMode)
            {
                flipped.Clear();
                int pathCount = dataTree.PathCount;
                int branchCount = dataTree.get_Branch(0).Count;

                for (int i = 0; i < pathCount; i++)
                {
                    for (int j = 0; j < branchCount; j++)
                    {
                        flipped.Append(dataTree.get_Branch(i)[j] as GH_Mesh, new GH_Path(j, i));
                    }
                }

                flipped.Flatten();
            }

            // Set output data
            if (simulationMode)
            {
                DA.SetDataTree(0, flipped);
            }
            else
            {
                DA.SetDataTree(0, hullMeshTree);
            }
            DA.SetDataTree(1, vertices);
            DA.SetDataTree(2, constraintPointIndices);
        }

        /// <summary>
        /// Dynamic variables.
        /// </summary>
        Random rd = new Random();
        GH_Structure<GH_Mesh> dataTree = new GH_Structure<GH_Mesh>();
        GH_Structure<GH_Mesh> flipped = new GH_Structure<GH_Mesh>();

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
            get { return new Guid("E478BB08-85AA-4025-803D-810B347FE8F2"); }
        }
    }
}