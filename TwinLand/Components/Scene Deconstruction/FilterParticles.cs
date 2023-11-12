using System;
using System.Collections.Generic;
using FlexCLI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Deconstruction
{
    public class FilterParticles : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the SortParticles class.
        /// </summary>
        public FilterParticles()
          : base("Filter Particles", "filter particles",
              "", "Deconstruction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "position", "", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Velocity", "velocity", "", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Group Indices", "group indices", "", GH_ParamAccess.list);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "position", "", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Velocity", "velocity", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> groupIndices = new List<int>();
            GH_Structure<GH_Point> posTree = new GH_Structure<GH_Point>();
            GH_Structure<GH_Vector> velTree = new GH_Structure<GH_Vector>();

            DA.GetDataTree("Position", out posTree);
            DA.GetDataTree("Velocity", out velTree);
            DA.GetDataList("Group Indices", groupIndices);

            GH_Structure<GH_Point> positions = new GH_Structure<GH_Point>();
            GH_Structure<GH_Vector> velocities = new GH_Structure<GH_Vector>();

            foreach (int index in groupIndices)
            {
                GH_Path path = new GH_Path(index);
                foreach (GH_Point p in posTree.get_Branch(index))
                {
                    positions.Append(p, path);
                }
                if (velTree.PathExists(path))
                {
                    foreach (GH_Vector v in velTree.get_Branch(index))
                    {
                        velocities.Append(v, path);
                    }
                }
            }

            DA.SetDataTree(0, positions);
            DA.SetDataTree(1, velocities);
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
            get { return new Guid("7A3054FE-6A16-43DD-84DE-247CC5EAE2EB"); }
        }
    }
}