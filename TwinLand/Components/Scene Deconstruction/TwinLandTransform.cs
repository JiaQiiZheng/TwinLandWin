using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Deconstruction
{
    public class TwinLandTransform : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Transform class.
        /// </summary>
        public TwinLandTransform()
          : base("Transform", "transform",
              "", "Deconstruct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Geometry", "geometry", "", GH_ParamAccess.list);
            pManager.AddTransformParameter("Transform", "transform", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "geometry", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> geos = new List<Mesh>();
            List<Transform> xforms = new List<Transform>();
            if (!DA.GetDataList("Geometry", geos)) return;
            if (!DA.GetDataList("Transform", xforms)) return;

            for (int i = 0; i < geos.Count; i++)
            {
                geos[i].Transform(xforms[i]);
            }

            DA.SetDataList("Geometry", geos);
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
            get { return new Guid("1943FEA7-1F82-4091-88C5-2036E9931445"); }
        }
    }
}