using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Principal;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.ApplicationSettings;
using System.Diagnostics;
using Rhino;

namespace TwinLand.Components.Mapping
{
    public class CustomizeGrasshopperWindow : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the CustomizeGrasshopperWindow class.
        /// </summary>
        public CustomizeGrasshopperWindow()
          : base("Customize Grasshopper Window", "customize gh window",
              "", "Mapping")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Opacity", "opacity", "", GH_ParamAccess.item, 1.0);
            pManager.AddColourParameter("Color", "color", "", GH_ParamAccess.item, Color.White);
            pManager.AddBooleanParameter("Apply", "apply", "", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double opacity = 1.0;
            DA.GetData("Opacity", ref opacity);
            opacity = Math.Max(0.0, opacity);
            opacity = Math.Min(opacity, 1.0);

            Color color = Color.White;
            DA.GetData("Color", ref color);


            bool apply = false;
            DA.GetData("Apply", ref apply);

            System.Windows.Forms.Form form = Instances.DocumentEditor;


            if (apply)
            {
                form.Opacity = opacity;
            }
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
                return Properties.Resources.T_icon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ACB608EB-3A97-4146-AE99-F0A8E1C8050F"); }
        }
    }
}