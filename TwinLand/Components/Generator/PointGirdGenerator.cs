using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TwinLand.Components.Generator
{
    public class PointGirdGenerator : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the PointGirdGenerator class.
        /// </summary>
        public PointGirdGenerator()
          : base("TwinLand Point Gird Generator", "TL point grid generator",
              "", "Generator")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "plane", "", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntervalParameter("X", "x", "", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Y", "y", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("X Count", "x count", "", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Y Count", "y count", "", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Pattern", "pattern", "0: Normal point grid, 1: Cross point grid", GH_ParamAccess.item, 0);
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
            Plane plane = Plane.WorldXY;
            Interval x_domain = Interval.Unset;
            Interval y_domain = Interval.Unset;
            int x_count = 2;    
            int y_count = 2;
            int pattern = 0;

            if (!DA.GetData("Plane", ref plane)) return;
            if (!DA.GetData("X", ref x_domain)) return;
            if (!DA.GetData("Y", ref y_domain)) return;
            DA.GetData("X Count", ref x_count);
            DA.GetData("Y Count", ref y_count);
            DA.GetData("Pattern", ref pattern);

            GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();

            if (pattern == 0)
            {
                // regular point grid
                double x_size = x_domain.Length;
                double y_size = y_domain.Length;

                double x_step = x_size / (x_count - 1);
                double y_step = y_size / (y_count - 1);

                for (int i = 0; i < x_count; i++)
                {
                    double param_x = x_step * i;
                    for (int j = 0; j < y_count; j++)
                    {
                        double param_y = y_step * j;
                        Point3d pt = plane.PointAt(param_x - 0.5 * x_size, param_y - 0.5 * y_size);

                        GH_Path path = new GH_Path(i, j);
                        pts.Append(new GH_Point(pt), path);
                    }
                }
            }
            else if (pattern == 1)
            {
                // cross point grid
                double x_size = x_domain.Length;
                double y_size = y_domain.Length;

                double x_step = x_size / (x_count - 1);
                double y_step = y_size / (y_count - 1);
                
                for(int i = 0;i < x_count;i+=1)
                {
                    int start = 0;
                    if (i % 2 != 0)
                    {
                        start = 1;
                    }

                    double param_x = x_step * i;
                    for (int j = start; j < y_count; j+=2)
                    {
                        double param_y = y_step * j;
                        Point3d pt = plane.PointAt(param_x - 0.5 * x_size, param_y - 0.5 * y_size);

                        GH_Path path = new GH_Path(i, j);
                        pts.Append(new GH_Point(pt), path);
                    }
                }
            }

            DA.SetDataTree(0, pts);

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
            get { return new Guid("03CE5503-FDE7-4D37-B876-36DA97C2D921"); }
        }
    }
}