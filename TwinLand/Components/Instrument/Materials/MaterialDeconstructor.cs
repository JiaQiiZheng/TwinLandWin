using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument.Materials
{
    public class MaterialDeconstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the MaterialDeconstructor class.
        /// </summary>
        public MaterialDeconstructor()
          : base("Material Deconstructor", "material deconstructor",
              "Description", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "material", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Solid Particle", "solid particle", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Fluid Particle", "fluid particle", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            MaterialObject material = null;
            if (!DA.GetData("Material", ref material)) return;

            DA.SetData("Solid Particle", material.SolidParticle);
            DA.SetData("Fluid Particle", material.FluidParticle);
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
            get { return new Guid("89BB1843-21A6-4A7C-B6A5-0D57ECB490A3"); }
        }
    }
}