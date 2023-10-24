﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Types.Transforms;
using MIConvexHull;
using OsmSharp.IO.PBF;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using TwinLand.Utils;

namespace TwinLand.Components.Generator
{
    public class ParticleCover : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the SurfaceParticleGenerator class.
        /// </summary>
        public ParticleCover()
          : base("Particle Cover", "particle cover",
              "Generator particle as meshes covering above surface, configure and run simulation to reach to a stable shape.", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "geometry", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Spacing", "spacing", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("TwinLand Particle", "TL particle", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Layer Thickness", "layer thickness", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale Factor", "scale factor", "", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Strict", "strict", "", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Origin", "origin", "", GH_ParamAccess.tree);
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
            List<IGH_GeometricGoo> geometryList = new List<IGH_GeometricGoo>();
            double spacing = 1.0;
            TwinLandParticle TL_particle = null;
            double layerThickness = 1;
            bool strict = true;
            double scaleFactor = 1.0;
            int maxCountPerAxis = 100;

            if (!DA.GetDataList("Geometry", geometryList)) return;
            if (!DA.GetData("Spacing", ref spacing)) return;
            if (!DA.GetData("TwinLand Particle", ref TL_particle)) return;
            if (!DA.GetData("Layer Thickness", ref layerThickness)) return;
            DA.GetData("Strict", ref strict);
            DA.GetData("Scale Factor", ref scaleFactor);

            // Get properties from TL_particle object
            double maxWidth = TL_particle.MaximumWidth;
            double maxLength = TL_particle.MaximumLength;
            double maxHeight = TL_particle.MaximumHeight;
            int vertexCount = TL_particle.VertexCount;
            double sizeEvenFactor = TL_particle.SizeEvenFactor;

            // Collect different types of geometry for projection
            BoundingBox bb = BoundingBox.Empty;

            List<Mesh> mesh_list = new List<Mesh>();
            List<Brep> brep_list = new List<Brep>();

            foreach (var geometry in geometryList)
            {
                string geoType = geometry.GetType().Name;
                if (geoType == "GH_Brep")
                {
                    GH_Brep gh_brep = geometry as GH_Brep;
                    brep_list.Add(gh_brep.Value);
                }
                if (geoType == "GH_Mesh")
                {
                    GH_Mesh gh_mesh = geometry as GH_Mesh;
                    mesh_list.Add(gh_mesh.Value);
                }

                bb.Union(geometry.Boundingbox);
            }
            Brep[] breps = brep_list.ToArray();
            Mesh[] meshes = mesh_list.ToArray();

            Transform xform = Transform.Scale(bb.Center, scaleFactor);
            bb.Transform(xform);

            // Calculate box domain
            Point3d min = bb.Min;
            Point3d max = bb.Max;

            Interval x_domain = new Interval(min.X, max.X);
            Interval y_domain = new Interval(min.Y, max.Y);

            // Prevent spacing slider set to value that is too small caused crashing
            if (x_domain.Length / (maxWidth + spacing) > maxCountPerAxis || y_domain.Length / (maxLength + spacing) > maxCountPerAxis)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Spacing is too small for an efficient lattice generation.");
                return;
            }

            // Get x y coordinate list for grid
            List<double> xs = CollectXYZ(x_domain, maxWidth+spacing);
            List<double> ys = CollectXYZ(y_domain, maxLength+spacing);

            // Hilistic cross-reference and create point grid
            List<Point3d> pts = new List<Point3d>();
            for (int j = 0; j < ys.Count; j++)
            {
                for (int k = 0; k < xs.Count; k++)
                {
                    pts.Add(new Point3d(xs[k], ys[j], 0));
                }
            }
            Point3d[] grid = pts.ToArray();

            // project onto geometry
            Point3d[] projected_brep = Intersection.ProjectPointsToBreps(breps, grid, Plane.WorldXY.ZAxis, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            Point3d[] projected_mesh = Intersection.ProjectPointsToMeshes(meshes, grid, Plane.WorldXY.ZAxis, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            List<Point3d> projected_list = new List<Point3d>();
            if (projected_brep != null)
            {
                foreach (Point3d pt in projected_brep)
                {
                    projected_list.Add(pt);
                }
            }

            if (projected_mesh != null)
            {
                foreach (Point3d pt in projected_mesh)
                {
                    projected_list.Add(pt);
                }
            }

            Point3d[] projected = projected_list.ToArray();
            Point3d.CullDuplicates(projected, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            // Lift projected point times based on thickness
            double layerCount = Math.Ceiling(layerThickness / maxHeight);
            List<Point3d> total = new List<Point3d>();
            for (int i = 1; i <= layerCount; i++)
            {
                foreach (Point3d pt in projected)
                {
                    total.Add(new Point3d(pt.X, pt.Y, pt.Z + i * (maxHeight+spacing)));
                }
            }

            int groupCount = total.Count;
            GH_Structure<GH_Point> origins = new GH_Structure<GH_Point>();

            for (int i = 0; i < groupCount; i++)
            {
                GH_Path groupPath = new GH_Path(i);
                origins.Append(new GH_Point(total[i]), groupPath);
            }

            // Initial mesh particle related output variables
            GH_Structure<GH_Mesh> hullMeshTree = new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Point> vertices = new GH_Structure<GH_Point>();
            GH_Structure<GH_Integer> constraintPointIndices = new GH_Structure<GH_Integer>();
            int constraintPointIndex = -1;

            // Generate mesh particles
            for (int i = 0; i < groupCount; i++)
            {
                Plane plane = new Plane(total[i], new Vector3d(0, 0, 1));
                IList<double[]> pointsCoordinates = new List<double[]>();
                GH_Path groupPath = new GH_Path(i);

                double sizeFactor = sizeEvenFactor + rd.NextDouble() * (1 - sizeEvenFactor);
                // Generate vertices
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

                // Create 3d convex hull
                var hull = ConvexHull.Create(pointsCoordinates, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                // Convert hull to rhino mesh
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

                hullMeshTree.Append(new GH_Mesh(hullMesh), groupPath);
            }

            // output
            DA.SetDataTree(0, origins);
            DA.SetDataTree(1, hullMeshTree);
            DA.SetDataTree(2, vertices);
            DA.SetDataTree(3, constraintPointIndices);
        }

        private List<double> CollectXYZ(Interval domain, double step)
        {
            List<double> collection = new List<double>();
            double start = domain.Min;

            while (start <= domain.Max)
            {
                collection.Add(start);
                start += step;
            }

            return collection;
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
            get { return new Guid("AC21F352-7704-4A86-B391-4D7CF05D1739"); }
        }
    }
}