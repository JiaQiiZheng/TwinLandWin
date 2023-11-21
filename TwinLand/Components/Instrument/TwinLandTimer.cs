using System;
using System.Timers;
using Grasshopper.Kernel;
using System.Threading;
using System.Threading.Tasks;
using Rhino;
using System.Diagnostics;

namespace TwinLand.Components.Instrument
{
    public class TwinLandTimer : TwinLandComponent
    {
        private System.Timers.Timer timer;
        /// <summary>
        /// Initializes a new instance of the TwinLandTrigger class.
        /// </summary>
        public TwinLandTimer()
          : base("TwinLand Timer", "TL timer",
              "Description", "Instrument")
        {
            timer = new System.Timers.Timer(interval); // 1000 milliseconds = 1 second
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Interval", "interval", "", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Start", "start", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("End", "end", "", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Step", "step", "", GH_ParamAccess.item, 0.1);
            pManager.AddBooleanParameter("Active", "active", "", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Reset", "reset", "", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "output", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int cur_interval = 1000;
            double start = 0.0;
            double end = 1.0;
            double step = 0.1;
            bool active = true;
            bool reset = false;

            DA.GetData("Interval", ref cur_interval);
            DA.GetData("Start", ref start);
            DA.GetData("End", ref end);
            DA.GetData("Step", ref step);
            DA.GetData("Active", ref active);
            DA.GetData("Reset", ref reset);

            if (!active) return;

            // Check if interval input is being changed
            if(cur_interval != interval)
            {
                UpdateTimer(timer, cur_interval);
                timer.Start();
            }

            if (reset)
            {
                output = 0;
                UpdateTimer(timer, cur_interval);
                timer.Start();
            }

            output += step;

            if (output >= end)
            {
                timer.Stop();
                output = end;
                ExpireSolution(true);
            }

            // Output the current time
            DA.SetData("Output", output);
        }

        int interval = 1000;
        double output = 0.0;

        private void UpdateTimer(System.Timers.Timer timer, int newInterval)
        {
            timer = new System.Timers.Timer(interval);
            this.interval = newInterval;
            timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Expire the solution to trigger a recomputation every second
            Grasshopper.Instances.ActiveCanvas.Invoke(new Action(() => { ExpireSolution(true); }));
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
            get { return new Guid("F461FB98-DD7E-4055-9814-6DDBCC842142"); }
        }
    }
}