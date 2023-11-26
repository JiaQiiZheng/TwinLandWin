using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace TwinLand.Components.Mapping
{
    public class CustomLineType : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the CustomLineType class.
        /// </summary>
        public CustomLineType()
          : base("Custom Line Type", "custom line type",
              "", "Mapping")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "crv", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dash Length", "dash length", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Gap Length", "gap length", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Start Scale", "start scale", "", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("End Scale", "end scale", "", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Flip Start", "flip start", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Flip End", "flip end", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Apply Dash", "apply dash", "", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Apply Nibs", "apply nibs", "", GH_ParamAccess.item, true);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "crv", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("Start", "start", "", GH_ParamAccess.item);
            pManager.AddCurveParameter("End", "end", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = null;
            double dashLen = 20.0;
            double gapLen = 20.0;
            double startScale = 1.0;
            double endScale = 1.0;
            bool flipStart = false;
            bool flipEnd = false;
            bool applyDash = true;
            bool applyNibs = true;

            if (!DA.GetData("Curve", ref crv)) return;
            DA.GetData("Dash Length", ref dashLen);
            DA.GetData("Gap Length", ref gapLen);
            DA.GetData("Start Scale", ref startScale);
            DA.GetData("End Scale", ref endScale);
            DA.GetData("Flip Start", ref flipStart);
            DA.GetData("Flip End", ref flipEnd);
            DA.GetData("Apply Dash", ref applyDash);
            DA.GetData("Apply Nibs", ref applyNibs);

            // Initialize output seg collection
            List<Curve> outputSegs = new List<Curve>();

            // Adjust dash pattern in case output is too heavy
            double total = crv.GetLength();

            // Make sure crv domain match to its length
            crv.Domain = new Interval(0, total);

            int maxSegCount = 1000;
            dashLen = Math.Max(dashLen, total / maxSegCount);
            gapLen = Math.Max(gapLen, total / maxSegCount);

            // Generate dash curves
            if (applyDash && dashLen != 0 && gapLen != 0)
            {
                double cur = 0;
                List<double> splitParams = new List<double>();
                Curve[] segs = null;

                while (cur < total)
                {
                    splitParams.Add(cur);
                    cur += dashLen;
                    splitParams.Add(cur);
                    cur += gapLen;
                }

                segs = crv.Split(splitParams.ToArray());

                // Cull gap segs
                for (int i = 0; i < segs.Length; i += 2)
                {
                    outputSegs.Add(segs[i]);
                }
            }

            // Generate nibs
            Curve arrowStart = null;
            Curve arrowEnd = null;

            if (applyNibs)
            {
                // Draw the start shape
                Point3d start = crv.PointAtStart;
                Point3d end = crv.PointAtEnd;
                Vector3d t_start = crv.TangentAtStart;
                Vector3d t_end = crv.TangentAtEnd;
                double len_start = Math.Max(startScale, total / 200);
                double len_end = Math.Max(endScale, total / 200);
                double angle = 30;

                arrowStart = DrawArrow(start, t_start, len_start, angle, flipStart);
                arrowEnd = DrawArrow(end, t_end, len_end, angle, flipEnd);
            }

            DA.SetDataList("Curve", outputSegs);
            DA.SetData("Start", arrowStart);
            DA.SetData("End", arrowEnd);
        }

        public Curve DrawArrow(Point3d origin, Vector3d tagent, double Length, double angle, bool flip)
        {
            if (flip)
            {
                tagent *= -1;
            }

            Vector3d leftDirection = -tagent;
            Vector3d rightDirection = -tagent;

            leftDirection.Rotate(-RhinoMath.ToRadians(angle), Vector3d.ZAxis);
            rightDirection.Rotate(RhinoMath.ToRadians(angle), Vector3d.ZAxis);

            Line left = new Line(origin, origin + leftDirection * Length);
            Line right = new Line(origin, origin + rightDirection * Length);
            Curve[] joined = Curve.JoinCurves(new Curve[2] { left.ToNurbsCurve(), right.ToNurbsCurve() });
            
            if (joined.Length != 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to draw arrow");
            }

            return joined[0];
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
            get { return new Guid("A7AFBABA-4154-450D-8D31-BB1C92AC77E2"); }
        }
    }
}