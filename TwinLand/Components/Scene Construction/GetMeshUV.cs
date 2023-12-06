using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper.Kernel;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Construction
{
    public class GetMeshUV : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the GetMeshUV class.
        /// </summary>
        public GetMeshUV()
          : base("Get Mesh Information", "get mesh information",
              "Description", "Construction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("U Count", "u count", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("V Count", "v count", "", GH_ParamAccess.item);
            pManager.AddIntervalParameter("U Interval", "u interval", "", GH_ParamAccess.item);
            pManager.AddIntervalParameter("V Interval", "v interval", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            if (!DA.GetData("Mesh", ref mesh)) return;

            double[] meshInfo = TwinLand.MeshInfo.GetMeshInfo(mesh);
            int u_count = (int)meshInfo[2];
            int v_count = (int)meshInfo[3];
            double u_interval = meshInfo[4];
            double v_interval = meshInfo[5];

            DA.SetData("U Count", u_count);
            DA.SetData("V Count", v_count);
            DA.SetData("U Interval", u_interval);
            DA.SetData("V Interval", v_interval);
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
            get { return new Guid("7F1BE861-57E4-4894-86F8-C12DBD6EA900"); }
        }
    }
}