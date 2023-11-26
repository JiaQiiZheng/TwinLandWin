using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using TwinLand.Utils;

namespace TwinLand.Components.Generator
{
    public class ParticleCover : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ParticleCover class.
        /// </summary>
        public ParticleCover()
          : base("Particle Cover", "particle cover",
              "Description", "Generator")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundary", "boundary", "", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Topography", "topography", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Diameter", "diameter", "", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Sparsity", "sparsity", "", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Layer", "layer", "", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Location", "location", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Curve> boundaries = null;
            Mesh topo = null;
            double diameter = 1.0;
            double sparsity = 1.0;
            int layer = 1;

            if (!DA.GetDataTree("Boundary", out boundaries)) return;
            if (!DA.GetData("Topography", ref topo)) return;
            if (!DA.GetData("Diameter", ref diameter)) return;
            DA.GetData("Sparsity", ref sparsity);
            DA.GetData("Layer", ref layer);

            sparsity = Math.Max(sparsity, 1.0);
            layer = Math.Max(layer, 1);

            GH_Structure<GH_Point> locationTree = new GH_Structure<GH_Point>();

            for (int i = 0; i < boundaries.Branches.Count; i++)
            {
                GH_Path path = boundaries.get_Path(i);

                List<Point3d> pts = new List<Point3d>();

                for (int j = 0; j < boundaries.get_Branch(path).Count; j++)
                {
                    Curve crv = boundaries.get_DataItem(path, j).Value;

                    if (!crv.IsClosed) continue;
                    Brep[] bps = Brep.CreatePlanarBreps(crv, tl);

                    foreach (Brep brep in bps)
                    {
                        Surface sur = brep.Faces[0].ToNurbsSurface();

                        // Calculate appropriate uv count based on input configurations
                        BoundingBox bb = crv.GetBoundingBox(true);
                        double width = bb.Max.X - bb.Min.X;
                        double length = bb.Max.Y - bb.Min.Y;
                        int uCount = Math.Min(uvMaxCount, (int)(width / (diameter * sparsity)));
                        int vCount = Math.Min(uvMaxCount, (int)(length / (diameter * sparsity)));

                        List<Point3d> grids = DivideSurfaceIntoGrid(sur, crv, uCount, vCount, true);

                        // Project points onto topo
                        Point3d[] projected = Intersection.ProjectPointsToMeshes(new Mesh[] { topo }, grids, Vector3d.ZAxis, tl);

                        // Lift up to layers
                        List<Point3d> layers = new List<Point3d>();
                        for (int k = 0; k < layer; k++)
                        {
                            double lift = (0.5 + k) * diameter;
                            foreach (Point3d p in projected)
                            {
                                Point3d newPt = new Point3d(p);
                                newPt.Z += lift;
                                layers.Add(newPt);
                            }
                        }

                        pts.AddRange(layers);
                    }
                }

                foreach (Point3d pt in pts)
                {
                    locationTree.Append(new GH_Point(pt), path);
                }
            }

            DA.SetDataTree(0, locationTree);
        }

        /// <summary>
        ///  Dynamic variables
        /// </summary>
        double tl = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
        int uvMaxCount = 100;
        Random rd = new Random();

        /// <summary>
        /// Methods
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="uCount"></param>
        /// <param name="vCount"></param>
        /// <returns></returns>
        private List<Point3d> DivideSurfaceIntoGrid(Surface surface, Curve crv, int uCount, int vCount, bool inside)
        {
            List<Point3d> points = new List<Point3d>();

            if (surface == null || uCount <= 0 || vCount <= 0)
                return points;

            // Reparameterize surface
            surface.SetDomain(0, new Interval(0, 1));
            surface.SetDomain(1, new Interval(0, 1));

            for (int i = 0; i < uCount; i++)
            {
                for (int j = 0; j < vCount; j++)
                {
                    double u = i / (double)(uCount - 1);
                    double v = j / (double)(vCount - 1);

                    Point3d point = surface.PointAt(u, v);

                    // Cull points outside of boundary
                    if (inside)
                    {
                        PointContainment pc = crv.Contains(point, Plane.WorldXY, tl);
                        if (pc != PointContainment.Inside)
                        {
                            continue;
                        }
                    }

                    points.Add(point);
                }
            }

            return points;
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
            get { return new Guid("D5AB197A-303E-419C-9A27-B83DEA3ED214"); }
        }
    }
}