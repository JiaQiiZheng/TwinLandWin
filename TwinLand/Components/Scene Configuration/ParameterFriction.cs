using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using FlexCLI;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Configuration
{
    public class ParameterFriction : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ParameterFriction class.
        /// </summary>
        public ParameterFriction()
          : base("Parameter Friction", "param friction",
              "", "Configuration")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// Parameter Friction
            // Dynamic Friction
            pManager.AddNumberParameter("Coefficient of Dynamic Friction", "coefficient of dynamic friction",
              "Coefficient of friction used when colliding objects have dynamic movement", GH_ParamAccess.item, 0.0);
            // Static Friction
            pManager.AddNumberParameter("Coefficient of Static Friction", "coefficient of static friction",
              "Coefficient of friction used when colliding objects are relatively static", GH_ParamAccess.item, 0.0);
            // Particle Friction
            pManager.AddNumberParameter("Coefficient of Particle Friction", "coefficient of particle friction",
              "Coefficient of friction used in particles' collision", GH_ParamAccess.item, 0.0);
            // Restitution
            pManager.AddNumberParameter("Restitution", "restitution",
              "Coefficient of restitution used when colliding with shapes. Particle collision are always inelastic",
              GH_ParamAccess.item, 0.0);
            // Adhesion
            pManager.AddNumberParameter("Adhesion", "adhesion", "Adhesion affects how both fluid and solid particles stick to solid surfaces. It will enable particles stick to and slide down surfaces",
              GH_ParamAccess.item, 0.0);
            // Shock Propagation
            pManager.AddNumberParameter("Shock Propagation", "ShockPropagation", "Artificially decrease the mass of particles based on height from a fixed reference point, this makes stacks and piles converge faster.", GH_ParamAccess.item, 0.0);
            // Dissipation
            pManager.AddNumberParameter("Dissipation", "dissipation",
              "Indicate a factor to damp the velocity of a particle based on how many particles it collide with",
              GH_ParamAccess.item, 0.0);
            // Damping
            pManager.AddNumberParameter("Damping", "damping",
              "The viscous drag force which is opposite to the particle velocity", GH_ParamAccess.item, 0.0);

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
            pManager.AddGenericParameter("Parameter Friction", "param friction", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FlexParams paramFriction = new FlexParams();

            // Friction
            double df = 0.0;
            double sf = 0.0;
            double pf = 0.0;
            double res = 0.0;
            double adh = 0.0;
            double shp = 0.0;
            double dis = 0.0;
            double dam = 0.0;

            DA.GetData("Coefficient of Dynamic Friction", ref df);
            DA.GetData("Coefficient of Static Friction", ref sf);
            DA.GetData("Coefficient of Particle Friction", ref pf);
            DA.GetData("Restitution", ref res);
            DA.GetData("Adhesion", ref adh);
            DA.GetData("Shock Propagation", ref shp);
            DA.GetData("Dissipation", ref dis);
            DA.GetData("Damping", ref dam);

            paramFriction.DynamicFriction = (float)df;
            paramFriction.StaticFriction = (float)sf;
            paramFriction.ParticleFriction = (float)pf;
            paramFriction.Restitution = (float)res;
            paramFriction.Adhesion = (float)adh;
            paramFriction.ShockPropagation = (float)shp;
            paramFriction.Dissipation = (float)dis;
            paramFriction.Damping = (float)dam;

            DA.SetData("Parameter Friction", paramFriction);
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
            get { return new Guid("9596640D-F3EE-4CAB-8B13-8D0996706D87"); }
        }
    }
}