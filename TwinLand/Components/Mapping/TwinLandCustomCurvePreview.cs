using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Getters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TwinLand.Components.Mapping
{
    public class TwinLandCustomCurvePreview : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the TwinLandCustomCurvePreview class.
        /// </summary>
        public TwinLandCustomCurvePreview()
          : base("TwinLand Custom Curve Preview", "TL custom curve preview",
              "", "Mapping")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "curve", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Thickness", "thickness", "", GH_ParamAccess.item, 1);
            pManager.AddColourParameter("Color", "color", "", GH_ParamAccess.item, Color.DarkGray);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "curve", "", GH_ParamAccess.item);
            pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Curve gh_crv = null;
            Color color = new Color();
            int thickness = 1;

            if (!DA.GetData("Curve", ref gh_crv)) return;
            DA.GetData("Color", ref color);
            DA.GetData("Thickness", ref thickness);

            _clip = BoundingBox.Union(_clip, gh_crv.Boundingbox);

            _curves.Add(gh_crv.Value);
            _colors.Add(color);
            _thinkness.Add(thickness);

            DA.SetData("Curve", gh_crv);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        private BoundingBox _clip;
        private readonly List<Curve> _curves = new List<Curve>();
        private readonly List<Color> _colors = new List<Color>();
        private readonly List<int> _thinkness = new List<int>();

        /// <summary>
        /// Additional methods
        /// </summary>
        /// <param name="args"></param>
        protected override void BeforeSolveInstance()
        {
            _clip = BoundingBox.Empty;
            _curves.Clear();
            _colors.Clear();
            _thinkness.Clear();
        }

        public override BoundingBox ClippingBox
        {
            get { return _clip; }
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);

            for (int i = 0; i < _curves.Count; i++)
            {
                args.Display.DrawCurve(_curves[i], _colors[i], _thinkness[i]);
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
            get { return new Guid("FEA413F4-B57D-4E84-98CD-44540A4927B5"); }
        }
    }
}