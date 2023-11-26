using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using TwinLand.Utils;

namespace TwinLand.Components.Instrument.Planar_Emitter
{
    public class PlnarPointEmitterDeconstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the PlnarPointEmitterDeconstructor class.
        /// </summary>
        public PlnarPointEmitterDeconstructor()
          : base("Plnar Point Emitter Deconstructor", "planar point emitter deconstructor",
              "", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Planar Point Emitter", "planar point emitter", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "points", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "velocities", "", GH_ParamAccess.list);
            pManager.AddRectangleParameter("Emitting Boundary", "emitting boundary", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData("Planar Point Emitter", ref instrument)) return;

            PlanarPointEmitter ppe = instrument.PlanarPointEmitter;
            if (ppe == null) return;

            DA.SetDataList("Points", ppe.Points);
            DA.SetDataList("Velocities", ppe.Velocities);
            DA.SetData("Emitting Boundary", ppe.EmittingBoundary);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        InstrumentObject instrument = null;


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
            get { return new Guid("55CAC62D-C493-41FA-90CF-0554E86EA93E"); }
        }
    }
}