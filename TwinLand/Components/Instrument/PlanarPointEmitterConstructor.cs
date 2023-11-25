using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using TwinLand.Utils;

namespace TwinLand.Components.Instrument
{
    public class PlanarPointEmitterConstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the PlanarPointEmitterConstructor class.
        /// </summary>
        public PlanarPointEmitterConstructor()
          : base("PlanarPointEmitterConstructor", "planar point emitter constructor",
              "", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Emitting Area Width", "emitting area width", "", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Emitting Area Height", "emitting area height", "", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Count", "count", "", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Random Factor", "random factor", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Velocity", "velocity", "initial velocity of each point", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Is Random", "is random", "", GH_ParamAccess.item, true);

            for (int i = 1; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Planar Point Emitter", "planar point emitter", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane plane = Plane.WorldXY;
            double width = 1.0;
            double height = 1.0;
            int count = 1;
            double randomFactor = 0.0;
            double velocity = 1.0;
            bool isRandom = true;
            Rectangle3d emittingBoundary = new Rectangle3d();

            DA.GetData("Emitting Area Width", ref width);
            DA.GetData("Emitting Area Height", ref height);
            DA.GetData("Count", ref count);
            DA.GetData("Random Factor", ref randomFactor);
            DA.GetData("Velocity", ref velocity);
            DA.GetData("Is Random", ref isRandom);

            PlanarPointEmitter ppe = new PlanarPointEmitter(plane, width, height, count, randomFactor, velocity, isRandom, emittingBoundary);
            InstrumentObject instrument = new InstrumentObject(ppe);

            DA.SetData("Planar Point Emitter", instrument);
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
            get { return new Guid("BCF1C188-C571-4BC5-9BC8-C2692621A02B"); }
        }
    }
}