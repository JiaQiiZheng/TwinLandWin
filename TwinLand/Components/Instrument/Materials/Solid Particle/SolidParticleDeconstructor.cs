using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument.Materials.Solid_Particle
{
    public class SolidParticleDeconstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the SolidParticleDeconstructor class.
        /// </summary>
        public SolidParticleDeconstructor()
          : base("Solid Particle Deconstructor", "solid particle deconstructor",
              "Description", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Solid Particle", "solid particle", "", GH_ParamAccess.item);
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
            SolidParticle sp = null;

            DA.GetData("Solid Particle", ref sp);
            if (sp == null) return;

            DA.SetData("Diameter", sp.Diameter);
            DA.SetData("Mass", sp.Mass);
            DA.SetData("Self Collision", sp.SelfCollision);
            DA.SetData("Group Index", sp.GroupIndex);
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
            get { return new Guid("3D2F3B80-6B63-45D4-A4FE-44C033FAFA59"); }
        }
    }
}