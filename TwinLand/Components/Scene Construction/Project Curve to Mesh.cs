using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using ProtoBuf.WellKnownTypes;
using Rhino;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Construction
{
    public class Project_Curve_to_Mesh : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Project_Curve_to_Mesh class.
        /// </summary>
        public Project_Curve_to_Mesh()
          : base("Project Curve to Mesh", "project curve to mesh",
              "Project curve to mesh based on direction", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "curve", "", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Direction", "direction", "", GH_ParamAccess.item, new Vector3d(0,0,-1));

            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "curve", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Curve> curveTree = new GH_Structure<GH_Curve>();
            Mesh mesh = null;
            Vector3d dir = new Vector3d(0, 0, -1);

            if (!DA.GetDataTree("Curve", out curveTree)) { return; }
            if (!DA.GetData("Mesh", ref mesh)) { return; }
            DA.GetData("Direction", ref dir);

            GH_Structure<GH_Curve> crvs = new GH_Structure<GH_Curve>();

            if (dir.IsValid)
            {
                for (int i = 0; i < curveTree.Branches.Count; i++)
                {
                    GH_Path path = curveTree.get_Path(i);
                    List<Curve> groupCrvs = new List<Curve> ();
                    for (int j = 0; j < curveTree.get_Branch(path).Count; j++)
                    {
                        Curve[] crvArr = Curve.ProjectToMesh(curveTree.get_DataItem(path, j).Value, mesh, dir, tl);
                        foreach (var crv in crvArr)
                        {
                            groupCrvs.Add(crv);
                        }
                    }

                    Curve[] joined = Curve.JoinCurves(groupCrvs);
                    foreach (Curve joinedCrv in joined)
                    {
                        crvs.Append(new GH_Curve(joinedCrv), path);
                    }
                }
            }

            DA.SetDataTree(0, crvs);
        }

        /// <summary>
        /// Static variables
        /// </summary>
        double tl = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

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
            get { return new Guid("1A6BB78A-1F93-4CA8-8F3D-EEBC7E8AF849"); }
        }
    }
}