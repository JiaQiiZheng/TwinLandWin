using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TwinLand.Utils;

namespace TwinLand.Components.Scene_Construction
{
    public class SurfaceParticleGenerator : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the SurfaceParticleGenerator class.
        /// </summary>
        public SurfaceParticleGenerator()
          : base("Surface Particle Generator", "surface particle generator",
              "Generator particle as meshes covering above surface, configure and run simulation to reach to a stable shape.", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Spacing", "spacing", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("TwinLand Particle", "TL particle", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Layer Count", "layer count", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.tree);
            pManager.AddPointParameter("Vertices", "vertices", "", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Constraint Point Indices", "constraint point indices", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            double spacing = 1.0;
            TwinLandParticle TL_particle = null;
            int layerCount = 1;

            

            // Generate a surface grid based on spacing and particle size
            
            // Move the grid up times based on layer count
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
            get { return new Guid("AC21F352-7704-4A86-B391-4D7CF05D1739"); }
        }
    }
}