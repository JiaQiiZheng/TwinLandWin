using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Deconstruction
{
    public class RigidBodyRigister : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the RigidBodyRigister class.
        /// </summary>
        public RigidBodyRigister()
          : base("Rigid Body Rigister", "rigid boby rigister",
              "", "Deconstruction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rigid Body", "rigid body", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Rigid Body Mesh", "rigid body mesh", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Group Rigid Body Count", "group rigid body count", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<RigidBody> rigids = new List<RigidBody>();
            if (!DA.GetDataList("Rigid Body", rigids)) return;

            List<GeometryBase> geos = new List<GeometryBase>();
            List<int> counts = new List<int>();

            int groupCount = rigids.Count;
            for (int i = 0; i < groupCount; i++)
            {
                geos.Add(rigids[i].Mesh);
            }
            counts.Add(groupCount);

            DA.SetDataList("Rigid Body Mesh", geos);
            DA.SetDataList("Group Rigid Body Count", counts);
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
            get { return new Guid("28A691FC-D0F7-4E2C-AD0D-7BA436A2B023"); }
        }
    }
}