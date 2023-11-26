using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument.Charactor
{
    public class CharactorDeconstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the CharactorDeconstructor class.
        /// </summary>
        public CharactorDeconstructor()
          : base("Charactor Deconstructor", "Charactor Deconstructor",
              "Description", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Charactor", "charactor", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Location", "location", "", GH_ParamAccess.item);
            pManager.AddMeshParameter("Body", "body", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Orientation", "orientation", "", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "color", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData("Charactor", ref charactor)) return;
            int[] argb = charactor.BodyColor[charactor.ColorIndex];
            Color cur_color = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);

            DA.SetData("Location", charactor.BodyLocation);
            DA.SetData("Body", charactor.BodyStatus);
            DA.SetData("Orientation", charactor.Orientation);
            DA.SetData("Color", cur_color);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        CharactorObject charactor = null;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
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
            get { return new Guid("383C69F3-1C1A-4AFE-BA8A-7849C7C2D7D6"); }
        }
    }
}