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
            pManager.AddGenericParameter("Parameters", "Parameters", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Collider", "Collider", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force Fields", "Force Fields", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Scenes", "Scenes", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Constraint", "Constraint", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Solver Options", "Solver Options", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Run", "Run", "", GH_ParamAccess.item, false);

            for (int i = 0; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
                if (i == 3 || i == 4)
                {
                    pManager[i].DataMapping = GH_DataMapping.Flatten;
                }
            }
        }

        /// <summary>
        /// initialize time related values
        /// </summary>
        Flex flex = new Flex();
        int counter = 0;
        Stopwatch sw_1 = new Stopwatch();
        Stopwatch sw_2 = new Stopwatch();
        long totalTime_ms = 0;
        long totalUpdateTime_ms = 0;
        List<string> log = new List<string>();

        // time stamps
        int options_ts = 0;
        int params_ts = 0;
        List<int> scene_tss = new List<int>();
        List<int> constraint_tss = new List<int>();
        List<int> forceField_tss = new List<int>();
        int collider_ts = 0;

        Task<int> updateTask;

        /// <summary>
        /// static method
        /// </summary>
        /// <returns></returns>
        private int Update()
        {
            sw_2.Restart();
            flex.UpdateSolver();
            return (int)sw_2.ElapsedMilliseconds;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FleX", "FleX", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("FleX_Log", "FleX_Log", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            updateTask = new Task<int>(() => Update());

            FlexParams param = new FlexParams();
            FlexCollisionGeometry collisonGeometry = new FlexCollisionGeometry();
            List<FlexForceField> forceField = new List<FlexForceField>();
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