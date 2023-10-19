using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Construction
{
    public class SurfacePointEmitter : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the GeometryPointEmitter class.
        /// </summary>
        public SurfacePointEmitter()
          : base("Surface Point Emitter", "surface point emitter",
              "Emit points based on normals of input surface", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "surface", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U Count", "u u_count", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("V Count", "v u_count", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Random Factor", "random factor", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Velocity", "velocity", "initial velocity of each point", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Is Random", "is random", "", GH_ParamAccess.item, true);
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
            Surface srf = null;
            int u_count = 1;
            int v_count = 1;
            double randomFactor = 0.0;
            double velocity = 1.0;
            bool isRandom = true;

            if (!DA.GetData("Surface", ref srf)) { return; }
            DA.GetData("U Count", ref u_count);
            DA.GetData("V Count", ref v_count);
            DA.GetData("Random Factor", ref randomFactor);
            DA.GetData("Velocity", ref velocity);
            DA.GetData("Is Random", ref isRandom);

            // declare a point list & array and vector list & array to contain points located on geometry
            List<Point3d> pts = new List<Point3d>();
            List<Vector3d> vts = new List<Vector3d>();

            if (u_count < 1 || v_count <1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The uv count needs to be positive");
                return;
            }

            // reparameterize surface
            Interval domainU = srf.Domain(0);
            Interval domainV = srf.Domain(1);

            if (isRandom)
            {
                for (int u = 0; u <= u_count; u++)
                {
                    for (int v = 0; v <= v_count; v++)
                    {
                        double u_param = domainU.ParameterAt(rd.NextDouble());
                        double v_param = domainV.ParameterAt(rd.NextDouble());

                        Point3d pt = srf.PointAt(u_param, v_param);
                        pts.Add(pt);
                        Vector3d normal = srf.NormalAt(u_param, v_param);
                        Plane pl = new Plane(pt, normal);
                        normal.Rotate((rd.NextDouble() - 0.5) * 2 * randomFactor, pl.XAxis);
                        vts.Add(normal*velocity);
                    }
                }
            }

            else
            {

                for (int u = 0; u <= u_count; u++)
                {
                    double u_param = domainU.ParameterAt(u / (double)u_count);
                    for (int v = 0; v <= v_count; v++)
                    {
                        double v_param = domainV.ParameterAt(v / (double)v_count);

                        Point3d pt = srf.PointAt(u_param, v_param);
                        pts.Add(pt);
                        Vector3d normal = srf.NormalAt(u_param, v_param);
                        Plane pl = new Plane(pt, normal);
                        normal.Rotate((rd.NextDouble() - 0.5) * 2 * randomFactor, pl.XAxis);
                        vts.Add(normal*velocity);
                    }
                }
            }

            DA.SetDataList(0, pts);
            DA.SetDataList(1, vts);
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
            get { return new Guid("D5BD9042-CCFE-4E01-B60B-EB4F15695D0F"); }
        }
    }
}