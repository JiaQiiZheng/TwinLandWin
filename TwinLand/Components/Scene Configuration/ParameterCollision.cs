using System;
using System.Collections.Generic;
using FlexCLI;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Configuration
{
    public class ParameterCollision : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ParameterCollision class.
        /// </summary>
        public ParameterCollision()
          : base("Parameter Collision", "param collision",
              "", "Configuration")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// Parameter Collision
            // Solid Rest Distance
            pManager.AddNumberParameter("Solid Rest Distance", "solid rest distance",
              "Parameter controls the distance solid particles attempt to maintain from each other, it must be less than or equal to the interaction radius.", GH_ParamAccess.item, 0.15);
            // Fluid Rest Distance
            pManager.AddNumberParameter("Fluid Rest Distance", "fluid rest distance",
              "Parameter controls the distance fluid particles attempt to maintain from each other, it must be less than or equal to the interaction radius.", GH_ParamAccess.item, 0.1);
            // Collision Distance
            pManager.AddNumberParameter("Collision Distance", "collision distance",
              "The distance particles attempt to maintain from colliders, to triangle mesh colliders, this parameter need to be positive value to advoid particles \"slipping\" through meshes due to numerical precision errors", GH_ParamAccess.item, 0.875);
            // Particle Collision Margin
            pManager.AddNumberParameter("Particle Collision Margin", "particle collision margin",
              "Expand particles' collision distance based on interaction radius, an apropriate value could advoid missing collison but affecting perfermance, need to be set as low as possible.",
              GH_ParamAccess.item, 0.5);
            // Shape Collision Margin
            pManager.AddNumberParameter("Shape Collision Margin", "shape collision margin",
              "Expand shapes' collision distance, an apropriate value could advoid missing collison but affecting perfermance, need to be set as low as possible.", GH_ParamAccess.item, 0.5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameter Collision", "param collision", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FlexParams paramCollision = new FlexParams();

            // collision
            double srd = 0.075;
            double frd = 0.075;
            double cd = 0.0;
            double pcm = 0.0;
            double scm = 0.0;

            DA.GetData("Solid Rest Distance", ref srd);
            DA.GetData("Fluid Rest Distance", ref frd);
            DA.GetData("Collision Distance", ref cd);
            DA.GetData("Particle Collision Margin", ref pcm);
            DA.GetData("Shape Collision Margin", ref scm);

            paramCollision.SolidRestDistance = (float)srd;
            paramCollision.FluidRestDistance = (float)frd;
            paramCollision.CollisionDistance = (float)cd;
            paramCollision.ParticleCollisionMargin = (float)pcm;
            paramCollision.ShapeCollisionMargin = (float)scm;

            DA.SetData("Parameter Collision", paramCollision);
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
            get { return new Guid("A0A721DE-58F1-4864-9D1F-03B944E1D848"); }
        }
    }
}