using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.FleX_Construct
{
    public class PlanarPointEmitter : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the PointEmitter class.
        /// </summary>
        public PlanarPointEmitter()
          : base("Planar Point Emitter", "planar point emitter",
              "Emit points in different ways based on input plane and emitting area.",
              "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Emitting Plane", "plane", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Emitting Area Width", "emitting area width", "", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Emitting Area Height", "emitting area height", "", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Count", "count", "", GH_ParamAccess.item, 1);
            //pManager.AddNumberParameter("Radius", "radius", "", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Random Factor", "random factor", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Velocity", "velocity", "initial velocity of each point", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Is Random", "is random", "", GH_ParamAccess.item, true);

            for (int i = 1; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "pts", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocity", "velocity", "", GH_ParamAccess.list);
            pManager.AddRectangleParameter("Emitting Boundary", "emitting boundary", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane plane = Plane.Unset;
            double width = 1.0;
            double height = 1.0;
            int count = 1;
            //double radius = 1.0;
            double randomFactor = 0.0;
            double velocity = 1.0;
            bool isRandom = true;
            Rectangle3d emittingBoundary = new Rectangle3d();

            if (!DA.GetData("Emitting Plane", ref plane)) { return; }
            DA.GetData("Emitting Area Width", ref width);
            DA.GetData("Emitting Area Height", ref height);
            DA.GetData("Count", ref count);
            //DA.GetData("Radius", ref radius);
            DA.GetData("Random Factor", ref randomFactor);
            DA.GetData("Velocity", ref velocity);
            DA.GetData("Is Random", ref isRandom);

            if (count <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The count needs to be positive");
                return;
            }

            // generate emitting boundary
            emittingBoundary = new Rectangle3d(plane, new Interval(-width / 2, width / 2), new Interval(-height / 2, height / 2));

            List<Point3d> pts = new List<Point3d>();
            List<Vector3d> vls = new List<Vector3d>();

            if (isRandom)
            {
                for (int i = 0; i < count; i++)
                {
                    // random in circle area
                    //pts.Add(new Point3d(plane.PointAt((rd.NextDouble() - 0.5) * radius, (rd.NextDouble() - 0.5) * radius)));

                    // random in rectangle area
                    pts.Add(new Point3d(plane.PointAt((rd.NextDouble() - 0.5) * width, (rd.NextDouble() - 0.5) * height)));

                    Vector3d vv = plane.ZAxis;
                    vv.Unitize();
                    vv *= velocity;
                    Vector3d rotA = plane.XAxis;

                    // rotate initial velocity based on random factor
                    rotA.Rotate(rd.NextDouble() * Math.PI * 2, plane.ZAxis);

                    vv.Rotate((rd.NextDouble() - 0.5) * 2 * randomFactor, rotA);

                    vls.Add(vv);
                }
            }

            else
            {
                // grid pattern based on input count as the row for emitter
                List<double> param_x = new List<double>();
                List<double> param_y = new List<double>();

                double step = 1.0 / count;
                for (int i = 0; i <= count; i++)
                {
                    param_x.Add(-0.5 + i * step);
                }

                double count_y = height / width * count;
                double step_y = width / height * step;
                for (int i = 0; i < count_y; i++)
                {
                    param_y.Add(-0.5 + i * step_y);
                }

                // cross-reference
                foreach (double x in param_x)
                {
                    foreach (double y in param_y)
                    {
                        pts.Add(new Point3d(plane.PointAt(x * width, y * height)));
                    }
                }

                Vector3d vv = plane.ZAxis;
                vv.Unitize();
                vv *= velocity;
                Vector3d rotA = plane.XAxis;

                // rotate initial velocity based on random factor
                rotA.Rotate(rd.NextDouble() * Math.PI * 2, plane.ZAxis);

                for (int j = 0; j < pts.Count; j++)
                {
                    vv.Rotate((rd.NextDouble() - 0.5) * 2 * randomFactor, rotA);
                    vls.Add(vv);
                }
            }

            DA.SetDataList(0, pts);
            DA.SetDataList(1, vls);
            DA.SetData("Emitting Boundary", emittingBoundary);
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
                return Properties.Resources.TL_Engine;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("BC35E067-DC58-46EE-BD4D-2C6953823A7F"); }
        }
    }
}