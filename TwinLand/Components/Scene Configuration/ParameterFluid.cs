using System;
using System.Collections.Generic;
using System.Reflection;
using FlexCLI;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Configuration
{
    public class ParameterFluid : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ParameterFluid class.
        /// </summary>
        public ParameterFluid()
          : base("Parameter Fluid", "param fluid",
              "", "Configuration")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// Parameter Fluid
            // Viscosity
            pManager.AddNumberParameter("Viscosity", "viscosity", "Smooth particles velocities using the XSPH viscosity",
              GH_ParamAccess.item, 0.0);
            // Cohesion
            pManager.AddNumberParameter("Cohesion", "cohesion",
              "Cohesion acts between fluid particles to bring them towards the rest-distance. This creates gooey effects that cause long strands of fluid to formm", GH_ParamAccess.item, 0.025);
            // Surface Tension
            pManager.AddNumberParameter("Surface Tension", "surface tension",
              "Surface tension acts to minimize the surface area of a fluid. It is only visable while doing high resolution and small scale simulation like droplets splitting and merging. Note that it is expensive computationally", GH_ParamAccess.item, 0.0);
            // Free Surface Drag
            pManager.AddNumberParameter("Free Surface Drag", "free surface drags",
              "Drag force applied to boundary fluid particles", GH_ParamAccess.item, 0.0);
            // Buoyancy
            pManager.AddNumberParameter("Buoyancy", "buoyancy", "A scale factor for particle gravity under fluid status",
              GH_ParamAccess.item, 1.0);

            for (int i = 0; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameter Fluid", "param fluid", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FlexParams paramFluid = new FlexParams();


            double vis = 0.0;
            double coh = 0.0;
            double st = 0.0;
            double fsd = 0.0;
            double buo = 0.0;

            DA.GetData("Viscosity", ref vis);
            DA.GetData("Cohesion", ref coh);
            DA.GetData("Surface Tension", ref st);
            DA.GetData("Free Surface Drag", ref fsd);
            DA.GetData("Buoyancy", ref buo);

            paramFluid.Viscosity = (float)vis;
            paramFluid.Cohesion = (float)coh;
            paramFluid.SurfaceTension = (float)st;
            paramFluid.FreeSurfaceDrag = (float)fsd;
            paramFluid.Buoyancy = (float)buo;

            DA.SetData("Parameter Fluid", paramFluid);
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
            get { return new Guid("A228C28E-B2E5-4EFF-A75E-FCE3CD524924"); }
        }
    }
}