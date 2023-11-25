using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument
{
    public class TwinLandCounter : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Counter class.
        /// </summary>
        public TwinLandCounter()
          : base("TwinLand Counter", "TL counter",
              "Description", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Interval", "interval", "", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Start", "start", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("End", "end", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Step", "step", "", GH_ParamAccess.item, 0.1);
            pManager.AddBooleanParameter("Active", "Active", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset", "Rst", "", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Counter", "C", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int cur_interval = 1000;

            DA.GetData("Interval", ref cur_interval);
            DA.GetData("Start", ref start);
            DA.GetData("End", ref end);
            DA.GetData("Step", ref step);
            DA.GetData("Active", ref active);
            DA.GetData("Reset", ref reset);

            if (cur_interval <= 0) return;

            if (timer == null && active)
            {
                timer = new System.Timers.Timer();
                timer.Interval = cur_interval;
                timer.Elapsed += UpdateSolution;
            }

            if (timer == null) return;

            // Update timer when input interval change
            if (cur_interval != interval)
            {
                timer.Interval = cur_interval;
                interval = cur_interval;
            }

            if (reset) Reset();

            if (active && !timer.Enabled)
            {
                Start();
            }
            else if (!active || timer.Enabled && end != 0 && counter >= end)
            {
                Stop();
            }

            DA.SetData("Counter", counter);
        }

        public void Start()
        {
            timer.Start();
        }
        public void Stop()
        {
            timer.Stop();
        }
        public void Reset()
        {
            timer.Interval = interval;
            counter = 0;
        }
        public void Update()
        {
            counter += step;
        }

        public void UpdateSolution(object source, EventArgs e)
        {
            Update();
            Grasshopper.Instances.ActiveCanvas.Invoke(new Action(() => { ExpireSolution(true); })); ;
        }

        System.Timers.Timer timer;
        double counter;
        int interval;
        double start;
        double end;
        double step;
        bool active, reset;

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
            get { return new Guid("C729707B-DBD5-4DFD-A747-F8C2CE56C42B"); }
        }
    }
}