using System;
using System.Collections.Generic;
using FlexCLI;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Configuration
{
    public class ParameterGlobal : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ParameterGlobal class.
        /// </summary>
        public ParameterGlobal()
          : base("Parameter Global", "parameter global",
              "", "Configuration")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// Parameter Global
            // Gravity
            pManager.AddVectorParameter("Gravity Acceleration", "gravity acceleration", "Default value set to the gravity acceleration value on earth.", GH_ParamAccess.item,
             new Vector3d(0.0, 0.0, -9.807));
            // Radius
            pManager.AddNumberParameter("Interaction Radius", "interaction radius", "Interaction radius, particles closer than this distance will be able to affect each other.", GH_ParamAccess.item, 0.15);
            // Max Speed
            pManager.AddNumberParameter("Maximum Speed", "max speed",
              "Particles' velocity in each iteration will be limited by this value", GH_ParamAccess.item, float.MaxValue);
            // Max Acceleration
            pManager.AddNumberParameter("Maximum Acceleration", "max acceleration",
              "Particles' acceleration in each iteration will be limited by this value", GH_ParamAccess.item, 100.0);
            // Stop Threshold
            pManager.AddNumberParameter("Particle Stop Threshold", "particle stop threshold",
              "Indicate a value to stop iteration for a particle while its velocity smaller than the value",
              GH_ParamAccess.item, 0.0);
            // Plastic Stop Threshold
            pManager.AddNumberParameter("Plastic Stop Threshold", "plastic stop threshold",
              "Indicate a threshold value for a solid shape. Once its moving magnitude smaller that the value than stop iteration",
              GH_ParamAccess.item, 0.0);
            // Override Fluid Implementation
            pManager.AddBooleanParameter("Is Fluid", "is fluid",
             "Set to true will take particles in group index {0} as fluid and implement fluid algorithm",
             GH_ParamAccess.item, true);
            // Relaxation Mode
            pManager.AddBooleanParameter("Relaxation Mode", "relaxation mode",
              "Set to true to apply RelaxationLocal Mode, will have slower convergence but more reliable. Set to false to apply RelaxationGlobal Mode, will be faster but could lead to errors in complexity.", GH_ParamAccess.item,
              true);
            // Relaxation Factor
            pManager.AddNumberParameter("Relaxation Factor", "relaxation factor",
              "Increase the convergence rate of solver, could be use to improve the efficiency of RelaxationLocal Mode while larger than 1.0, but it will lead to error when too larges.", GH_ParamAccess.item, 1.0);


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
            pManager.AddGenericParameter("Parameter Global", "param global", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FlexParams paramGlobal = new FlexParams();


            Vector3d ga = new Vector3d(0.0, 0.0, -9.807);
            double iRadius = 0.15;
            double mxs = 0.0;
            double mxa = 0.0;
            double pst = 0.0;
            double sst = 0.0;
            bool flu = true;
            bool rm = true;
            double rf = 1.0;

            DA.GetData("Gravity Acceleration", ref ga);
            DA.GetData("Interaction Radius", ref iRadius);
            DA.GetData("Maximum Speed", ref mxs);
            DA.GetData("Maximum Acceleration", ref mxa);
            DA.GetData("Particle Stop Threshold", ref pst);
            DA.GetData("Plastic Stop Threshold", ref sst);
            DA.GetData("Is Fluid", ref flu);
            DA.GetData("Relaxation Mode", ref rm);
            DA.GetData("Relaxation Factor", ref rf);


            paramGlobal.GravityX = (float)ga.X;
            paramGlobal.GravityY = (float)ga.Y;
            paramGlobal.GravityZ = (float)ga.Z;
            paramGlobal.Radius = (float)iRadius;
            paramGlobal.MaxSpeed = (float)mxs;
            paramGlobal.MaxAcceleration = (float)mxa;
            paramGlobal.SleepThreshold = (float)pst;
            paramGlobal.PlasticThreshold = (float)sst;
            paramGlobal.Fluid = flu;
            paramGlobal.RelaxationMode = rm ? 1 : 0;
            paramGlobal.RelaxationFactor = (float)rf;

            DA.SetData("Parameter Global", paramGlobal);
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
            get { return new Guid("8728FCB8-B9E7-4CC1-890C-AF4F7946EB2C"); }
        }
    }
}