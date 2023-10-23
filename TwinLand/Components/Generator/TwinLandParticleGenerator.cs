using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using TwinLand.Utils;

namespace TwinLand.Components.Generator
{
    public class TwinLandParticleGenerator : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the TwinLandParticleGenerator class.
        /// </summary>
        public TwinLandParticleGenerator()
          : base("TwinLand Particle Generator", "TL particles generator",
              "Configuration of TwinLand Particle", "Generator")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Maximum Width", "max width", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Maximum Length", "max length", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Maximum Height", "max height", "", GH_ParamAccess.item);

            pManager.AddIntegerParameter("Vertex Count", "vertex count", "", GH_ParamAccess.item, 6);
            pManager.AddNumberParameter("Size Even Factor", "size even factor", "", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("TwinLand Particle", "TL particle", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double maxWidth = 1.0;
            double maxLength = 1.0;
            double maxHeight = 1.0;
            int vertexCount = 6;
            double sizeEvenFactor = 1.0;

            if (!DA.GetData("Maximum Width", ref maxWidth)) return;
            if (!DA.GetData("Maximum Length", ref maxLength)) return;
            if (!DA.GetData("Maximum Height", ref maxHeight)) return;
            if (maxWidth <= 0 || maxLength <= 0 || maxHeight <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid dimension to construct 3D geometry.");
            }

            DA.GetData("Vertex Count", ref vertexCount);
            if (vertexCount < 4)
            {
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Vertex Count needs to be larger than 4 to form a mesh.");
                    return;
                }
            }

            DA.GetData("Size Even Factor", ref sizeEvenFactor);
            if (sizeEvenFactor < 0.0 || sizeEvenFactor > 1.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Size Even Factor needs to be value between 0.0 and 1.0");
                return;
            }

            TwinLandParticle TL_particle = new TwinLandParticle(maxWidth, maxLength, maxHeight, vertexCount, sizeEvenFactor);

            DA.SetData("TwinLand Particle", TL_particle);
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
            get { return new Guid("E3744BEB-08E2-4971-96A9-F8E87C646F6A"); }
        }
    }
}