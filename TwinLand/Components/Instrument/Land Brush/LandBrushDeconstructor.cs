using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument.Land_Brush
{
    public class LandBrushDeconstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the LandBrushDeconstructor class.
        /// </summary>
        public LandBrushDeconstructor()
          : base("Land Brush Deconstructor", "land brush deconstructor",
              "Description", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Land Brush", "land brush", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Zone", "zone", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Stroke", "stroke", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "material", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            InstrumentObject instrument = null;
            if (!DA.GetData("Land Brush", ref instrument)) return;

            LandBrush lb = instrument.LandBrush;
            if (lb == null) return;

            DA.SetData("Zone", lb.Zone);
            DA.SetDataList("Stroke", lb.Stroke);
            DA.SetData("Material", lb.MaterialWrapper);
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
            get { return new Guid("CBA7CBAB-A305-4536-A2E2-239DA17663E4"); }
        }
    }
}