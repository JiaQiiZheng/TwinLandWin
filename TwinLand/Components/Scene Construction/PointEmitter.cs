using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.FleX_Construct
{
    public class PointEmitter : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the PointEmitter class.
        /// </summary>
        public PointEmitter()
          : base("Point Emitter", "pt emitter",
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
            pManager.AddIntegerParameter("Count", "count", "", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Radius", "radius", "", GH_ParamAccess.item, 1.0);
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
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane plane = Plane.Unset;
            int count = 1;
            double radius = 1.0;
            double randomFactor = 0.0;
            double velocity = 1.0;
            bool isRandom = true;

            if (!DA.GetData("Emitting Plane", ref plane)) { return; }
            DA.GetData("Count", ref count);
            DA.GetData("Radius", ref radius);
            DA.GetData("Random Factor", ref randomFactor);
            DA.GetData("Velocity", ref velocity);
            DA.GetData("Is Random", ref isRandom);

            if (count <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The count needs to be positive");
                return;
            }

            List<Point3d> pts = new List<Point3d>();
            List<Vector3d> vls = new List<Vector3d>();

            if (isRandom)
            {
                for (int i = 0; i < count; i++)
                {
                    pts.Add(new Point3d(plane.PointAt((rd.NextDouble() - 0.5) * radius, (rd.NextDouble() - 0.5) * radius)));
                }

                Vector3d vv = plane.ZAxis;
                vv.Unitize();
                vv *= velocity;
                Vector3d rotA = plane.XAxis;

                // rotate initial velocity based on random factor
                rotA.Rotate(rd.NextDouble() * Math.PI * 2, plane.ZAxis);

                vv.Rotate((rd.NextDouble() - 0.5) * 2 * randomFactor, rotA);
                vls.Add(vv);
            }

            else
            {
                // grid pattern based on input count as the row for emitter
                List<double> param_x = new List<double>();
                List<double> param_y = new List<double>();

                double step = 1.0 / count;
                for (int j = 0; j <= count; j++)
                {
                    param_x.Add(-0.5 + j * step);
                    param_y.Add(-0.5 + j * step);
                }

                // cross-reference
                foreach (double x in param_x)
                {
                    foreach (double y in param_y)
                    {
                        pts.Add(new Point3d(plane.PointAt(x * radius, y * radius)));
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
        }

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