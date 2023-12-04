using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument.Materials.Fluid_Particle
{
    public class FluidParticleDeconstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the FluidParticleDeconstructor class.
        /// </summary>
        public FluidParticleDeconstructor()
          : base("Fluid Particle Deconstructor", "fluid particle deconstructor",
              "Description", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Fluid Particle", "fluid particle", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Diameter", "diameter", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mass", "mass", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Self Collision", "self collision", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Group Index", "group index", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FluidParticle fp = null;

            DA.GetData("Fluid Particle", ref fp);
            if (fp == null) return;

            DA.SetData("Diameter", fp.Diameter);
            DA.SetData("Mass", fp.Mass);
            DA.SetData("Self Collision", fp.SelfCollision);
            DA.SetData("Group Index", fp.GroupIndex);
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
            get { return new Guid("54C9F08D-BEBB-4A21-A7D4-21F36422AEF8"); }
        }
    }
}