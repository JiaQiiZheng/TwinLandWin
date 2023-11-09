using System;
using System.Collections.Generic;
using FlexCLI;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Configuration
{
    public class ParameterSolid : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ParameterSolid class.
        /// </summary>
        public ParameterSolid()
          : base("Parameter Solid", "param solid",
              "", "Configuration")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            /// Parameter Solid
            // Solid Pressure
            pManager.AddNumberParameter("Solid Pressure", "solid pressure",
              "Solid pressure acts to control how intense a solid behave in collision", GH_ParamAccess.item, 1.0);
            // Plastic Creep
            pManager.AddNumberParameter("Plastic Creep", "plastic creep",
              "A coefficient controls the rate of a static solid had passed the stop threshold", GH_ParamAccess.item, 0.0);

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
            pManager.AddGenericParameter("Parameter Solid", "param solid", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FlexParams paramSolid = new FlexParams();

            // Solid
            double sc = 0.0;
            double sp = 0.0;

            DA.GetData("Plastic Creep", ref sc);
            DA.GetData("Solid Pressure", ref sp);

            paramSolid.PlasticCreep = (float)sc;
            paramSolid.SolidPressure = (float)sc;
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
            get { return new Guid("D8E5F7BB-DE82-4C6B-9FF4-8FC35604A3D6"); }
        }
    }
}