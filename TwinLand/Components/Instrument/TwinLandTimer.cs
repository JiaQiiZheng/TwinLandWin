using System;
using System.Timers;
using Grasshopper.Kernel;
using System.Threading;
using System.Threading.Tasks;
using Rhino;

namespace TwinLand.Components.Instrument
{
    public class TwinLandTimer : TwinLandComponent
    {
        private readonly System.Timers.Timer timer;
        /// <summary>
        /// Initializes a new instance of the TwinLandTrigger class.
        /// </summary>
        public TwinLandTimer()
          : base("TwinLand Timer", "TL timer",
              "Description", "Instrument")
        {
            timer = new System.Timers.Timer(1000); // 1000 milliseconds = 1 second
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Current Time", "Time", "Outputs the current time every second", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Perform any other computations if needed

            // Output the current time
            DA.SetData(0, DateTime.Now.ToString("HH:mm:ss"));
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