using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FlexCLI;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace TwinLand
{
    public class GetParticles : TwinLandComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GetParticles()
          : base("Get Particles", "get particles",
            "Get all particles from output object from solver.", "Deconstruct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FleX", "FleX", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Output Velocity", "output velocity", "", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Interval", "interval", "Display particles by indicating interval of solver iteration, large interval speed up the performance but lose smooth appearance", GH_ParamAccess.item, 1);
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
        /// start status
        /// </summary>
        private int interval = 1;
        private int counter = 0;


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            counter++;
            DA.GetData("Interval", ref interval);
            interval = Math.Max(1, interval);

            bool outputVelocity = false;
            DA.GetData("Output Velocity", ref outputVelocity);

            if (counter % interval == 0)
            {
                Flex flex = null;
                DA.GetData("FleX", ref flex);

                if (flex != null)
                {
                    List<FlexParticle> particles = flex.Scene.GetAllParticles();

                    positions = new GH_Structure<GH_Point>();
                    velocities = new GH_Structure<GH_Vector>();

                    if (outputVelocity)
                    {
                        foreach (FlexParticle fp in particles)
                        {
                            GH_Path path = new GH_Path(fp.GroupIndex);
                            positions.Append(new GH_Point(new Point3d(fp.PositionX, fp.PositionY, fp.PositionZ)), path);
                            velocities.Append(new GH_Vector(new Vector3d(fp.VelocityX, fp.VelocityY, fp.VelocityZ)), path);
                        }
                    }
                    else
                    {
                        foreach (FlexParticle fp in particles)
                        {
                            GH_Path path = new GH_Path(fp.GroupIndex);
                            positions.Append(new GH_Point(new Point3d(fp.PositionX, fp.PositionY, fp.PositionZ)), path);
                        }
                    }

                    DA.SetDataTree(0, positions);
                    DA.SetDataTree(1, velocities);
                }
            }
        }

        /// <summary>
        /// declare new empty tree, output empty when iteration not step onto the interval
        /// </summary>
        private GH_Structure<GH_Point> positions = new GH_Structure<GH_Point>();
        private GH_Structure<GH_Vector> velocities = new GH_Structure<GH_Vector>();

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
            get { return new Guid("728ade19-5d60-42fe-87d5-6e53e60ef520"); }
        }
    }
}