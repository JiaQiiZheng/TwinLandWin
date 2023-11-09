using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
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
    public class ParticleCoverFromGeometry : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the SurfaceParticleGenerator class.
        /// </summary>
        public ParticleCoverFromGeometry()
          : base("Particle Cover from Geometry", "particle cover from geometry",
              "Generator particle as meshes covering above surface, configure and run simulation to reach to a stable shape.", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundary", "boundary", "Optional boundary input, will be used to clip particle generate area", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Geometry", "geometry", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Spacing", "spacing", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("TwinLand Particle", "TL particle", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Layer Thickness", "layer thickness", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset Factor", "offset factor", "", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Strict", "strict", "", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Simulation Mode", "simulation mode", "", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Reset", "reset", "", GH_ParamAccess.item, false);

            pManager[0].Optional = true;
            pManager[0].DataMapping = GH_DataMapping.Flatten;
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

            pManager[1].DataMapping = GH_DataMapping.Flatten;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> boundaries = new List<Curve>();
            List<IGH_GeometricGoo> geometryList = new List<IGH_GeometricGoo>();
            double spacing = 1.0;
            TwinLandParticle TL_particle = null;
            double layerThickness = 1;
            bool strict = true;
            double scaleFactor = 1.0;
            int maxCountPerAxis = 1000;
            bool simulationMode = true;
            bool reset = false;

            DA.GetDataList("Boundary", boundaries);
            if (!DA.GetDataList("Geometry", geometryList)) return;
            if (!DA.GetData("Spacing", ref spacing)) return;
            if (!DA.GetData("TwinLand Particle", ref TL_particle)) return;
            if (!DA.GetData("Layer Thickness", ref layerThickness)) return;
            DA.GetData("Strict", ref strict);
            DA.GetData("Offset Factor", ref scaleFactor);
            DA.GetData("Simulation Mode", ref simulationMode);
            DA.GetData("Reset", ref reset);

            // Clear recorded data if reset
            if (reset)
            {
                recordMesh.Clear();
                recordConstraintIndices.Clear();
                return;
            }

            // Get properties from TL_particle object
            double maxWidth = TL_particle.MaximumWidth;
            double maxLength = TL_particle.MaximumLength;
            double maxHeight = TL_particle.MaximumHeight;
            int vertexCount = TL_particle.VertexCount;
            double sizeEvenFactor = TL_particle.SizeEvenFactor;

            // Use input boundaries to reduce the grid pionts count
            List<Curve> bound_flatten = new List<Curve>();
            foreach (Curve crv in boundaries)
            {
                // close crv to use intersection metho later
                if (crv != null && !crv.IsClosed)
                {
                    if (!crv.MakeClosed(10))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input boundaries has inclosed curves, cloud not be used as clipping boundary.");
                    }
                }

                bound_flatten.Add(Curve.ProjectToPlane(crv, Plane.WorldXY));
            }
            Curve[] bounds = null;
            var region = Curve.CreateBooleanRegions(bound_flatten.ToArray(), Plane.WorldXY, true, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            List<Curve> regions = new List<Curve>();
            for (int i = 0; i < region.PlanarCurveCount; i++)
            {
                regions.Add(region.PlanarCurve(i));
            }
            bounds = regions.ToArray();

            // Create multiple grid systems in different bounds
            List<Point3d> grid_list = new List<Point3d>();

            foreach (var bound in bounds)
            {
                BoundingBox bbox = bound.GetBoundingBox(true);

                Interval x_domain = new Interval(bbox.Min.X, bbox.Max.X);
                Interval y_domain = new Interval(bbox.Min.Y, bbox.Max.Y);

                if (x_domain.Length / (maxWidth + spacing) > maxCountPerAxis || y_domain.Length / (maxLength + spacing) > maxCountPerAxis)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Spacing is too small for an efficient lattice generation.");
                    return;
                }

                // Get x y coordinate list for grid
                List<double> xs = CollectXYZ(x_domain, maxWidth + spacing);
                List<double> ys = CollectXYZ(y_domain, maxLength + spacing);

                // Hilistic cross-reference and create point grid
                for (int j = 0; j < ys.Count; j++)
                {
                    for (int k = 0; k < xs.Count; k++)
                    {
                        Point3d pt = new Point3d(xs[k], ys[j], 0);
                        PointContainment result = bound.Contains(pt);
                        if (strict)
                        {
                            if (result == PointContainment.Inside)
                            {
                                grid_list.Add(pt);
                            }
                        }
                        else
                        {
                            if (result == PointContainment.Inside || result == PointContainment.Coincident)
                            {
                                grid_list.Add(pt);
                            }
                        }
                    }
                }
            }

            Point3d[] grid = grid_list.ToArray();

            //// Collect different types of geometry for projection
            //BoundingBox bbox_geometry = BoundingBox.Empty;

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

                //bbox_geometry.Union(geometry.Boundingbox);
            }

            Brep[] breps = brep_list.ToArray();
            Mesh[] meshes = mesh_list.ToArray();

            //Transform xform = Transform.Scale(bbox_geometry.Center, scaleFactor);
            //bbox_geometry.Transform(xform);

            //// Calculate box domain
            //Point3d min = bbox_geometry.Min;
            //Point3d max = bbox_geometry.Max;

            //Interval x_domain = new Interval(min.X, max.X);
            //Interval y_domain = new Interval(min.Y, max.Y);

            // Prevent spacing slider set to value that is too small caused crashing
            //if (x_domain.Length / (maxWidth + spacing) > maxCountPerAxis || y_domain.Length / (maxLength + spacing) > maxCountPerAxis)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Spacing is too small for an efficient lattice generation.");
            //    return;
            //}

            //// Get x y coordinate list for grid
            //List<double> xs = CollectXYZ(x_domain, maxWidth + spacing);
            //List<double> ys = CollectXYZ(y_domain, maxLength + spacing);

            //// Hilistic cross-reference and create point grid
            //List<Point3d> pts = new List<Point3d>();
            //for (int j = 0; j < ys.Count; j++)
            //{
            //    for (int k = 0; k < xs.Count; k++)
            //    {
            //        Point3d pt = new Point3d(xs[k], ys[j], 0);

            //        // Filter points are not inside boundary
            //        for (int i = 0; i < boundaries.Count; i++)
            //        {
            //            if (boundaries[i].Contains(pt) == PointContainment.Inside)
            //            {
            //                pts.Add(pt);
            //                continue;
            //            }
            //        }
            //    }
            //}

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
                    total.Add(new Point3d(pt.X, pt.Y, pt.Z + i * (maxHeight + spacing)));
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
                var hull = ConvexHull.Create(pointsCoordinates, 0.0001);

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

                // Append new round of data into record datasets
                int constraintIndex_curr = recordConstraintIndices.DataCount-1;
                if (simulationMode)
                {
                    GH_Path groupPath_curr = new GH_Path(recordMesh.Branches.Count);
                    recordMesh.Append(new GH_Mesh(hullMesh), groupPath_curr);
                    for (int j = 0; j < TL_particle.VertexCount; j++)
                    {
                        recordConstraintIndices.Append(new GH_Integer(++constraintIndex_curr), groupPath_curr);
                    }
                }
            }

            // output
            DA.SetDataTree(0, origins);
            if (simulationMode)
            {
                DA.SetDataTree(1, recordMesh);
                DA.SetDataTree(3, recordConstraintIndices);
            }
            else
            {
                DA.SetDataTree(1, hullMeshTree);
                DA.SetDataTree(3, constraintPointIndices);
            }
            DA.SetDataTree(2, vertices);
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
        GH_Structure<GH_Mesh> recordMesh = new GH_Structure<GH_Mesh>();
        GH_Structure<GH_Integer> recordConstraintIndices = new GH_Structure<GH_Integer>();


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