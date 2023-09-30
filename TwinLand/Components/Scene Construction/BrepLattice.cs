using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;

namespace TwinLand.Components.FleX_Construct
{
    public class BrepLattice : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the brepToPointCloud class.
        /// </summary>
        public BrepLattice()
          : base("Brep Lattice", "brep lattice",
              "Fill input breps with 3d point array.", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Brep", "brep", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Spacing", "spacing", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Strict", "strict", "", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "points", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> breps = new List<Brep>();
            double spacing = 1.0;
            bool strict = true;

            if(!DA.GetDataList("Brep", breps)) { return; }
            if(!DA.GetData("Spacing", ref spacing)) { return; }
            DA.GetData("Strict", ref strict);

            // get bounding box
            BoundingBox bbox = BoundingBox.Empty;
            foreach (Brep brep in breps)
            {
                bbox.Union(brep.GetBoundingBox(true));
            }

            // calculate box domain
            Point3d min = bbox.Min;
            Point3d max = bbox.Max;

            Interval x_domain = new Interval(min.X, max.X);
            Interval y_domain = new Interval(min.Y, max.Y);
            Interval z_domain = new Interval(min.Z, max.Z);

            // get sub points location
            int x_count = (int)Math.Ceiling(x_domain.Length / spacing);
            int y_count = (int)Math.Ceiling(y_domain.Length / spacing);
            int z_count = (int)Math.Ceiling(z_domain.Length / spacing);

            // get x y z list for points
            List<double> xs = CollectXYZ(x_domain.Min, x_count, spacing);
            List<double> ys = CollectXYZ(y_domain.Min, y_count, spacing);
            List<double> zs = CollectXYZ(z_domain.Min, z_count, spacing);

            // construct point cloud based on bounding box using cross-reference
            List<double> xs_ref = new List<double>();
            List<double> ys_ref = new List<double>();
            List<double> zs_ref = new List<double>();
            
            // Hilistic cross-reference
            for (int i = 0; i < zs.Count; i++)
            {
                double z_ref = zs[i];
                for (int j = 0; j < ys.Count; j++)
                {
                    double y_ref = ys[j];
                    for (int k = 0; k < xs.Count; k++)
                    {
                        double x_ref = xs[k];

                        // add to ref collections
                        xs_ref.Add(x_ref);
                        ys_ref.Add(y_ref);
                        zs_ref.Add(z_ref);
                    }
                }
            }

            // if point is inside brep, then add it to cloud
            GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();
            for (int i = 0; i < xs_ref.Count; i++)
            {
                Point3d pt = new Point3d(xs_ref[i], ys_ref[i], zs_ref[i]);
                
                // check if the point is inside any of the brep
                foreach(Brep b in breps)
                {
                    if (b.IsPointInside(pt, RhinoMath.SqrtEpsilon, strict))
                    {
                        pts.Append(new GH_Point(pt));
                        continue;
                    }
                }
            }

            DA.SetDataTree(0, pts);
        }

        /// <summary>
        /// static methods
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="spacing"></param>
        /// <returns></returns>
        private List<double> CollectXYZ(double start, int count, double spacing)
        {
            List<double> collection = new List<double>();
            for (int i = 0; i < count; i++)
            {
                collection.Add(start + i * spacing);
            }
            return collection;
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
            get { return new Guid("D0271DEE-96BC-47EE-BA1D-93CF2ADA5233"); }
        }
    }
}