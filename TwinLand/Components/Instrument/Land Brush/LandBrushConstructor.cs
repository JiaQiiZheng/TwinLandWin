using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using TwinLand.Components.Instrument.Materials;

namespace TwinLand.Components.Instrument.Land_Brush
{
    public class LandBrushConstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the LandBrushConstructor class.
        /// </summary>
        public LandBrushConstructor()
          : base("Land Brush Constructor", "land brush constructor",
              "Description", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Radius", "radius", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "material", "", GH_ParamAccess.item);
            pManager.AddMeshParameter("Topography", "topography", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Land Brush", "land brush", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetData("Radius", ref radius);
            DA.GetData("Material", ref materialWrapper);
            DA.GetData("Topography", ref topography);

            LandBrush lb = new LandBrush(radius, topography, tl, materialWrapper);
            InstrumentObject instrumentWrapper = new InstrumentObject(lb);

            DA.SetData("Land Brush", instrumentWrapper);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        double tl = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
        double radius = 1000.0;
        MaterialObject materialWrapper = null;
        Mesh topography = null;

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
            get { return new Guid("234CE2D9-622E-4D29-9085-8CE93D33EC2A"); }
        }
    }
}