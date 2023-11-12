using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FlexCLI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Deconstruction
{
    public class GetFluidParticles : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the GetFluidParticles class.
        /// </summary>
        public GetFluidParticles()
          : base("Get Fluid Particles", "get fluid particles",
              "", "Deconstruct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FleX", "FleX", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Output Velocity", "output velocity", "", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Interval", "interval", "", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "pt", "", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Velocity", "velocity", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            counter++;
            DA.GetData("Interval", ref interval);
            interval = Math.Max(1, interval);
            
            if(counter % interval == 0)
            {
                Flex flex = null;
                bool outputVelocity = false;
                
                DA.GetData("FleX", ref flex);
                DA.GetData("Output Velocity", ref outputVelocity);
                
                if (flex != null)
                {
                    List<FlexParticle> fluids = flex.Scene.GetFluidParticles();

                    positions = new GH_Structure<GH_Point>();
                    velocities = new GH_Structure<GH_Vector>();

                    if (!outputVelocity)
                    {
                        foreach (FlexParticle fl in fluids)
                        {
                            GH_Path path = new GH_Path(fl.GroupIndex);
                            positions.Append(new GH_Point(new Point3d(fl.PositionX, fl.PositionY, fl.PositionZ)), path);
                        }
                    }
                    else
                    {
                        foreach (FlexParticle fl in fluids)
                        {
                            GH_Path path = new GH_Path(fl.GroupIndex);
                            positions.Append(new GH_Point(new Point3d(fl.PositionX, fl.PositionY, fl.PositionZ)), path);
                            velocities.Append(new GH_Vector(new Vector3d(fl.VelocityX, fl.VelocityY, fl.VelocityZ)), path);
                        }
                    }
                }
            }

            DA.SetDataTree(0, positions);
            DA.SetDataTree(1, velocities);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        private int interval = 1;
        private int counter = 0;
        GH_Structure<GH_Point> positions = new GH_Structure<GH_Point>();
        GH_Structure<GH_Vector> velocities = new GH_Structure<GH_Vector>();
        
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
            get { return new Guid("FB29F757-9439-41BC-9BF0-306B49BC145A"); }
        }
    }
}