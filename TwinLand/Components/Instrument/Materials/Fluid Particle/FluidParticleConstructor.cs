using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument.Materials.Fluid_Particle
{
    public class FluidParticleConstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the FluidParticleConstructor class.
        /// </summary>
        public FluidParticleConstructor()
          : base("Fluid Particle Constructor", "fluid particle constructor",
              "", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Diameter", "diameter", "", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("Mass", "mass", "", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Self Collision", "self collision", "", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("Group Index", "group index", "", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Fluid Particle", "fluid particle", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double diameter = 10.0;
            double mass = 1.0;
            bool selfCollision = true;
            int groupIndex = 0;

            DA.GetData("Diameter", ref diameter);
            DA.GetData("Mass", ref mass);
            DA.GetData("Self Collision", ref selfCollision);
            DA.GetData("Group Index", ref groupIndex);

            FluidParticle sp = new FluidParticle(diameter, mass, selfCollision, groupIndex);
            MaterialObject materialWrapper = new MaterialObject(sp);
            DA.SetData("Fluid Particle", materialWrapper);
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
            get { return new Guid("B9FB3259-B887-4C8D-89CF-C60A3882F631"); }
        }
    }
}