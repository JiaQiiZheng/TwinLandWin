using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

using System.Diagnostics;
using System.Threading.Tasks;

using FlexCLI;

namespace TwinLand
{
    public class FleX_Options : TwinLandComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FleX_Options()
          : base("FleX_Options", "FleX_Options",
            "Set up options for FleX engine", "Solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Duration Time", "Duration Time", "", GH_ParamAccess.item, defaultDt);
            pManager.AddIntegerParameter("Sub Steps", "Sub Steps", "", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("Iteration", "Iteration", "", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("Scene Mode", "Scene Mode", "", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("FixedNum Total Iterations", "FixedNum Total Iterations", "", GH_ParamAccess.item, -1);
            pManager.AddIntegerParameter("Memory Requirements", "Memoery Requirements", "", GH_ParamAccess.list, defaultMemoryRequirements);
            pManager.AddNumberParameter("Stability Scaling Factor", "Stability Scaling Factor", "", GH_ParamAccess.item, 1);
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Options", "options", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// static values
        /// </summary>
        float defaultDt = (float)1 / (float)60;
        List<int> defaultMemoryRequirements = new List<int> { 131072, 96, 65536, 65536, 65536, 65536, 65536, 196608, 131972 };

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            float dt = defaultDt;
            int subSteps = 3;
            int numIterations = 3;
            int sceneMode = 0;
            int fixedNumTotalIterations = -1;
            List<int> memoryRequirements = new List<int>();
            float stabilityScalingFactor = 1;

            DA.GetData("Duration Time", ref dt);
            DA.GetData("Sub Steps", ref subSteps);
            DA.GetData("Iteration", ref numIterations);
            DA.GetData("Scene Mode", ref sceneMode);
            DA.GetData("FixedNum Total Iterations", ref fixedNumTotalIterations);
            DA.GetDataList("Memory Requirements", memoryRequirements);
            DA.GetData("Stability Scaling Factor", ref stabilityScalingFactor);

            if (dt == 0 || stabilityScalingFactor == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Duration Time or Stability Scaling Factor might invalid");
            }

            if (memoryRequirements.Count == 0 || memoryRequirements.Count != 9)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Memory Requirement list is invalid, default list was used");
                memoryRequirements = defaultMemoryRequirements;
            }

            DA.SetData("Options", new FlexSolverOptions(dt, subSteps, numIterations, sceneMode, fixedNumTotalIterations, memoryRequirements.ToArray(), (float)Math.Max(stabilityScalingFactor, 0001)));
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.TL_Engine;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b3df4f62-0d75-4eae-9711-97aba2845af4"); }
        }
    }
}