using System;
using System.Collections.Generic;
using FlexCLI;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TwinLand
{
    public class ParticlesFromPoints : TwinLandComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ParticlesFromPoints()
          : base("Particles from Points", "particles from pts",
            "Convert points to particle objects used by scene.", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "points", "Point or point cloud used to construct FleX particle object",
              GH_ParamAccess.tree);
            pManager.AddVectorParameter("Velocities", "velocities", "Initial velocities for particles", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mass", "mass",
              "Mass values for all particles or indicate one value for all particles", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Self Collision", "self collision",
              "Set to true will calculate collision for the same group of particles", GH_ParamAccess.tree, true);
            pManager.AddIntegerParameter("Group Index", "group index", "Index to identify a specific group later, if empty, then follow the tree branch index",
              GH_ParamAccess.tree);

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
            pManager.AddGenericParameter("Particles", "particles", "Constructed FleX particles", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> pointTree = new GH_Structure<GH_Point>();
            GH_Structure<GH_Vector> velocityTree = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Number> massTree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Boolean> selfCollisionTree = new GH_Structure<GH_Boolean>();
            GH_Structure<GH_Integer> groupIndexTree = new GH_Structure<GH_Integer>();

            if (!DA.GetDataTree("Points", out pointTree)) return;
            DA.GetDataTree("Velocities", out velocityTree);
            DA.GetDataTree("Mass", out massTree);
            DA.GetDataTree("Self Collision", out selfCollisionTree);
            DA.GetDataTree("Group Index", out groupIndexTree);

            #region clean up tree
            if (!pointTree.IsEmpty)
            {
                pointTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            }
            if (!velocityTree.IsEmpty)
            {
                velocityTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            }
            if (!massTree.IsEmpty)
            {
                massTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            }
            if (!selfCollisionTree.IsEmpty)
            {
                selfCollisionTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            }
            if (!groupIndexTree.IsEmpty)
            {
                groupIndexTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            }

            if (pointTree.Branches.Count == 1)
            {
                GH_Structure<GH_Point> pT = new GH_Structure<GH_Point> ();
                pT.AppendRange(pointTree.Branches[0], new GH_Path(0));
                pointTree = pT;
            }
            if (velocityTree.Branches.Count == 1)
            {
                GH_Structure<GH_Vector> vT = new GH_Structure<GH_Vector>();
                vT.AppendRange(velocityTree.Branches[0], new GH_Path(0));
                velocityTree = vT;
            }
            if (massTree.Branches.Count == 1)
            {
                GH_Structure<GH_Number> mT = new GH_Structure<GH_Number>();
                mT.AppendRange(massTree.Branches[0], new GH_Path(0));
                massTree = mT;
            }
            if (selfCollisionTree.Branches.Count == 1)
            {
                GH_Structure<GH_Boolean> sT = new GH_Structure<GH_Boolean>();
                sT.AppendRange(selfCollisionTree.Branches[0], new GH_Path(0));
                selfCollisionTree = sT;
            }
            if (groupIndexTree.Branches.Count == 1)
            {
                GH_Structure<GH_Integer> gT = new GH_Structure<GH_Integer>();
                gT.AppendRange(groupIndexTree.Branches[0], new GH_Path(0));
                groupIndexTree = gT;
            }
            #endregion

            List<FlexParticle> particles = new List<FlexParticle>();

            // loop through input points tree
            for (int i = 0; i < pointTree.PathCount; i++)
            {
                GH_Path path = new GH_Path(i);
                for (int j = 0; j < pointTree.get_Branch(path).Count; j++)
                {
                    // convert location xyz value into particles
                    float[] pos = new float[3]
                    {
            (float)pointTree.get_DataItem(path, j).Value.X, (float)pointTree.get_DataItem(path, j).Value.Y,
            (float)pointTree.get_DataItem(path, j).Value.Z
                    };

                    // convert velocity xyz vector into particles
                    float[] vel = new float[3] { 0.0f, 0.0f, 0.0f };
                    if (velocityTree.PathExists(path))
                    {
                        if (j < velocityTree.get_Branch(path).Count)
                        {
                            vel = new float[3]
                            {
                (float)velocityTree.get_DataItem(path, j).Value.X, (float)velocityTree.get_DataItem(path, j).Value.Y,
                (float)velocityTree.get_DataItem(path, j).Value.Z
                            };
                        }
                        else
                        {
                            vel = new float[3]
                            {
                (float)velocityTree.get_DataItem(path, 0).Value.X, (float)velocityTree.get_DataItem(path, 0).Value.Y,
                (float)velocityTree.get_DataItem(path, 0).Value.Z
                            };
                        }
                    }

                    // convert mass value to inverse mass value for all particles
                    float inverseMass = 1.0f;
                    if (massTree.PathExists(path))
                    {
                        if (j < massTree.get_Branch(path).Count)
                        {
                            inverseMass = inverseMass / (float)massTree.get_DataItem(path, j).Value;
                        }
                        else
                        {
                            inverseMass = inverseMass / (float)massTree.get_DataItem(path, 0).Value;
                        }
                    }

                    // convert self collision booleans to all particles
                    bool selfCollision = false;
                    if (selfCollisionTree.PathExists(path))
                    {
                        if (j < selfCollisionTree.get_Branch(path).Count)
                        {
                            selfCollision = selfCollisionTree.get_DataItem(path, j).Value;
                        }
                        else
                        {
                            selfCollision = selfCollisionTree.get_DataItem(path, 0).Value;
                        }
                    }

                    // convert isFluid booleans to all particles
                    bool isFluid = false;

                    // convert group index to particles
                    int groupIndex = i;
                    if (groupIndexTree.PathExists(path))
                    {
                        if (j < groupIndexTree.get_Branch(path).Count)
                        {
                            groupIndex = groupIndexTree.get_DataItem(path, j).Value;
                        }
                        else
                        {
                            groupIndex = groupIndexTree.get_DataItem(path, 0).Value;
                        }
                    }

                    // construct current particle and add to particles collection output
                    particles.Add(new FlexParticle(pos, vel, inverseMass, selfCollision, isFluid, groupIndex, true));
                }
            }

            DA.SetDataList("Particles", particles);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.TL_Engine;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("66066de0-3d81-4462-8397-faf69f965e2b"); }
        }
    }
}