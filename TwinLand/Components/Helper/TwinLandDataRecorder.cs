using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TwinLand.Components.Helper
{
    public class TwinLandDataRecorder : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the TwinLandDataRecorder class.
        /// </summary>
        public TwinLandDataRecorder()
          : base("TwinLand Data Recorder", "TL data recorder",
              "Improved data recorder that could be reset", "Helper")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "data", "", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Reset", "reset", "", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "data", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool reset = false;
            GH_Structure<IGH_Goo> data;

            DA.GetDataTree("Data", out data);
            DA.GetData("Reset", ref reset);

            if (reset)
            {
                dataTree.Clear();
                return;
            }

            for (int i = 0; i < data.PathCount; i++)
            {
                var list = data.get_Branch(i);
                for (int j = 0; j < list.Count; j++)
                {
                    dataTree.Append((IGH_Goo)list[j], new GH_Path(i, j));
                }
            }

            DA.SetDataTree(0, dataTree);
        }

        /// <summary>
        /// global variables
        /// </summary>
        GH_Structure<IGH_Goo> dataTree = new GH_Structure<IGH_Goo>();

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
            get { return new Guid("7D29D3A9-BAB0-4E63-A6A7-05D0C9F278B5"); }
        }
    }
}