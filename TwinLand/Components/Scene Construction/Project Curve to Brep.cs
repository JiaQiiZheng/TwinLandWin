using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Construction
{
    public class Project_Curve_to_Brep : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Project_Curve_to_Brep class.
        /// </summary>
        public Project_Curve_to_Brep()
          : base("Project Curve to Brep", "project curve to brep",
              "Project curve on brep based on direction", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "curve", "", GH_ParamAccess.item);
            pManager.AddBrepParameter("Brep", "brep", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Direction", "direction", "", GH_ParamAccess.item, new Vector3d(0,0,1));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "curve", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> curves = new List<Curve>();

            Curve curve = null;
            Brep brep = null;
            Vector3d dir = new Vector3d(0, 0, 1);

            if (!DA.GetData("Curve", ref curve)) { return; }
            if (!DA.GetData("Brep", ref brep)) { return; }
            DA.GetData("Direction", ref dir);

            Curve[] crvs = Curve.ProjectToBrep(curve, brep, dir, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            foreach (Curve crv in crvs)
            {
                curves.Add(crv);
            }

            DA.SetDataList("Curve", curves);
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
            get { return new Guid("63E778EF-FB65-424D-8384-7F718771F193"); }
        }
    }
}