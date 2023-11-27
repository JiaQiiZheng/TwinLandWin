using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument
{
    public class InstrumentConstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the InstrumentConstructor class.
        /// </summary>
        public InstrumentConstructor()
          : base("Instrument Constructor", "instrument constructor",
              "Description","Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Planar Emitter", "planar emitter", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Land Brush", "land brush", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Instrument", "instrument", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            InstrumentObject instrument = new InstrumentObject();
            InstrumentObject pe = null;
            InstrumentObject lb = null;

            DA.GetData("Planar Emitter", ref pe);
            DA.GetData("Land Brush", ref lb);
            
            // Collect specific instruments
            if(pe.PlanarEmitter!= null)
            {
                instrument.PlanarEmitter = pe.PlanarEmitter;
            }
            if(lb.LandBrush!= null)
            {
                instrument.LandBrush = lb.LandBrush;
            }

            DA.SetData("Instrument", instrument);
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
            get { return new Guid("326F5C81-7976-4420-992B-98D47AE9EA89"); }
        }
    }
}