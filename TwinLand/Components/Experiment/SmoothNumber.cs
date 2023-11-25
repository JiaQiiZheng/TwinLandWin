using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper.Kernel;
using ProtoBuf.WellKnownTypes;
using Rhino.Geometry;

namespace TwinLand.Components.Experiment
{
    public class SmoothNumber : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the SmoothNumber class.
        /// </summary>
        public SmoothNumber()
          : base("Smooth Number", "smooth number",
              "", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Start Value", "Start", "Starting value", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("End Value", "End", "Ending value", GH_ParamAccess.item, 100.0);
            pManager.AddNumberParameter("Duration", "Duration", "Transition duration in seconds", GH_ParamAccess.item, 5.0);
            pManager.AddBooleanParameter("Reset", "reset", "", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Current Value", "Current", "Current interpolated value", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool reset = false;

            if (!DA.GetData(0, ref startValue)) return;
            if (!DA.GetData(1, ref endValue)) return;
            if (!DA.GetData(2, ref duration)) return;
            DA.GetData(3, ref reset);

            if (reset) currentTime = 0;

            double t = EaseInOut(currentTime / duration);
            double currentValue = Lerp(startValue, endValue, t);

            DA.SetData(0, currentValue);

            if (currentTime < duration)
            {
                currentTime += 0.1; // Adjust the step based on your needs
                ExpireSolution(true);
            }

            ExpireSolution(true);
        }

        private double startValue = 0.0;
        private double endValue = 100.0;
        private double duration = 5.0; // in seconds
        private double currentTime = 0.0;

        private double EaseInOut(double t)
        {
            // You can use different easing functions for smoother transitions
            return t * t * (3.0 - 2.0 * t);
        }


        private double Lerp(double start, double end, double t)
        {
            return start + t * (end - start);
        }

        //private void Callback(object sender, EventArgs e)
        //{
        //    ExpireSolution(true);
        //}

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
            get { return new Guid("56E354C0-4548-43AE-AB37-E09EB9FC958E"); }
        }
    }
}