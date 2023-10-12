using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using FlexCLI;

namespace TwinLand.Components.Scene_Construction
{
    public class RigidFromShapeMatching : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the RigidFromShapeMatching class.
        /// </summary>
        public RigidFromShapeMatching()
          : base("Rigid from Shape Matching", "regid from shape matching",
              "Semi-rigid body constructed by a group of points keeping relatively static. You could control the rigid deformation by adjusting stiffness", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Constraint Point Indices", "constraint pt indices", "Indices of a group of particles that need to be shape-matching as semi-rigid", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stiffness", "stiffness", "Stiffness coeificient for each rigid body from 0.0 to 1.0, default set to 1.0", GH_ParamAccess.list, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constraint", "Constraint", "Shape-matching constriant for all rigid bodies used by scene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Integer> indexTree = new GH_Structure<GH_Integer> ();
            List<double> stiffs = new List<double> ();

            List<ConstraintSystem> constraints = new List<ConstraintSystem>();

            if(!DA.GetDataTree("Constraint Point Indices", out indexTree)) { return; }
            DA.GetDataList("Stiffness", stiffs);

            for (int i = 0; i < indexTree.Branches.Count; i++)
            {
                List<int> indices = new List<int>();
                foreach (GH_Integer index in indexTree.Branches[i])
                {
                    indices.Add(index.Value);
                }
                
                // use 1.0 as stiffness if not indicated
                if (indices.Count > 1)
                {
                    if(stiffs.Count > i && stiffs[i] >= 0.0 && stiffs[i] <= 1.0)
                    {
                        constraints.Add(new ConstraintSystem(indices.ToArray(), (float)stiffs[i]));
                    }
                    else
                    {
                        constraints.Add(new ConstraintSystem(indices.ToArray(), defaultStiffness));
                    }
                }
            }

            DA.SetDataList(0, constraints);
        }

        /// <summary>
        /// static values
        /// </summary>
        float defaultStiffness = 1.0f;

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
            get { return new Guid("8FA844E8-CBE7-44BD-AF38-340C449193C4"); }
        }
    }
}