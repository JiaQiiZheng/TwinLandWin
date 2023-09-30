using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FlexCLI;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;

namespace TwinLand
{
    public class Parameters : TwinLandComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Parameters()
          : base("Parameters", "parameters",
            "Customize environmental variables used by Solver. Save parameters as .xml by checking option menu.", "Configuration")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            #region MyRegion

            // Particle Properties
            // Gravity
            pManager.AddVectorParameter("Gravity Acceleration", "gravity acceleration", "Default value set to the gravity acceleration value on earth.", GH_ParamAccess.item,
              new Vector3d(0.0, 0.0, -9.807));
            // Radius
            pManager.AddNumberParameter("Interaction Radius", "interaction radius", "Interaction radius, particles closer than this distance will be able to affect each other.", GH_ParamAccess.item, 0.15);

            // Collision Parameters
            // Solid Rest Distance
            pManager.AddNumberParameter("Solid Rest Distance", "solid rest distance",
              "Parameter controls the distance solid particles attempt to maintain from each other, it must be less than or equal to the interaction radius.", GH_ParamAccess.item, 0.15);
            // Fluid Rest Distance
            pManager.AddNumberParameter("Fluid Rest Distance", "fluid rest distance",
              "Parameter controls the distance fluid particles attempt to maintain from each other, it must be less than or equal to the interaction radius.", GH_ParamAccess.item, 0.1);
            // Collision Distance
            pManager.AddNumberParameter("Collision Distance", "collision distance",
              "The distance particles attempt to maintain from colliders, to triangle mesh colliders, this parameter need to be positive value to advoid particles \"slipping\" through meshes due to numerical precision errors", GH_ParamAccess.item, 0.875);
            // Particle Collision Margin
            pManager.AddNumberParameter("Particle Collision Margin", "particle collision margin",
              "Expand particles' collision distance based on interaction radius, an apropriate value could advoid missing collison but affecting perfermance, need to be set as low as possible.",
              GH_ParamAccess.item, 0.5);
            // Shape Collision Margin
            pManager.AddNumberParameter("Shape Collision Margin", "shape collision margin",
              "Expand shapes' collision distance, an apropriate value could advoid missing collison but affecting perfermance, need to be set as low as possible.", GH_ParamAccess.item, 0.5);
            // Max Speed
            pManager.AddNumberParameter("Maximum Speed", "max speed",
              "Particles' velocity in each iteration will be limited by this value", GH_ParamAccess.item, float.MaxValue);
            // Max Acceleration
            pManager.AddNumberParameter("Maximum Acceleration", "max acceleration",
              "Particles' acceleration in each iteration will be limited by this value", GH_ParamAccess.item, 100.0);

            // Friction Parameters
            // Dynamic Friction
            pManager.AddNumberParameter("Coefficient of Dynamic Friction", "coefficient of dynamic friction",
              "Coefficient of friction used when colliding objects have dynamic movement", GH_ParamAccess.item, 0.0);
            // Static Friction
            pManager.AddNumberParameter("Coefficient of Static Friction", "coefficient of static friction",
              "Coefficient of friction used when colliding objects are relatively static", GH_ParamAccess.item, 0.0);
            // Particle Friction
            pManager.AddNumberParameter("Coefficient of Particle Friction", "coefficient of particle friction",
              "Coefficient of friction used in particles' collision", GH_ParamAccess.item, 0.0);
            // Restitution
            pManager.AddNumberParameter("Restitution", "restitution",
              "Coefficient of restitution used when colliding with shapes. Particle collision are always inelastic",
              GH_ParamAccess.item, 0.0);
            // Adhesion
            pManager.AddNumberParameter("Adhesion", "adhesion", "Adhesion affects how both fluid and solid particles stick to solid surfaces. It will enable particles stick to and slide down surfaces",
              GH_ParamAccess.item, 0.0);
            // Stop Threshold
            pManager.AddNumberParameter("Particle Stop Threshold", "particle stop threshold",
              "Indicate a value to stop iteration for a particle while its velocity smaller than the value",
              GH_ParamAccess.item, 0.0);
            // Shock Propagation
            pManager.AddNumberParameter("Shock Propagation", "ShockPropagation", "Artificially decrease the mass of particles based on height from a fixed reference point, this makes stacks and piles converge faster.", GH_ParamAccess.item, 0.0);
            // Dissipation
            pManager.AddNumberParameter("Dissipation", "dissipation",
              "Indicate a factor to damp the velocity of a particle based on how many particles it collide with",
              GH_ParamAccess.item, 0.0);
            // Damping
            pManager.AddNumberParameter("Damping", "damping",
              "The viscous drag force which is opposite to the particle velocity", GH_ParamAccess.item, 0.0);

            // Fluid Parameters
            // Fluid
            pManager.AddBooleanParameter("Is Fluid", "is fluid",
              "Set to true will take particles in group index {0} as fluid and implement fluid algorithm",
              GH_ParamAccess.item, true);
            // Viscosity
            pManager.AddNumberParameter("Viscosity", "viscosity", "Smooth particles velocities using the XSPH viscosity",
              GH_ParamAccess.item, 0.0);
            // Cohesion
            pManager.AddNumberParameter("Cohesion", "cohesion",
              "Cohesion acts between fluid particles to bring them towards the rest-distance. This creates gooey effects that cause long strands of fluid to formm", GH_ParamAccess.item, 0.025);
            // Surface Tension
            pManager.AddNumberParameter("Surface Tension", "surface tension",
              "Surface tension acts to minimize the surface area of a fluid. It is only visable while doing high resolution and small scale simulation like droplets splitting and merging. Note that it is expensive computationally", GH_ParamAccess.item, 0.0);
            // Solid Pressure
            pManager.AddNumberParameter("Solid Pressure", "solid pressure",
              "Solid tension acts to control how intense a solid behave in collision", GH_ParamAccess.item, 1.0);
            // Free Surface Drag
            pManager.AddNumberParameter("Free Surface Drag", "free surface drags",
              "Drag force applied to boundary fluid particles", GH_ParamAccess.item, 0.0);
            // Buoyancy
            pManager.AddNumberParameter("Buoyancy", "buoyancy", "A scale factor for particle gravity under fluid status",
              GH_ParamAccess.item, 1.0);

            // Solid Parameters
            // Plastic Stop Threshold
            pManager.AddNumberParameter("Plastic Stop Threshold", "plastic stop threshold",
              "Indicate a threshold value for a solid shape. Once its moving magnitude smaller that the value than stop iteration",
              GH_ParamAccess.item, 0.0);
            // Plastic Creep
            pManager.AddNumberParameter("Plastic Creep", "plastic creep",
              "A coefficient controls the rate of a static solid had passed the stop threshold", GH_ParamAccess.item, 0.0);

            // Cloth
            // Wind
            pManager.AddVectorParameter("Wind", "wind",
              "Constant acceleration applied to particles of cloth and inflatables.", GH_ParamAccess.item,
              new Vector3d(0.0, 0.0, 0.0));
            // Drag
            pManager.AddNumberParameter("Drag", "drag", "Drag force applied to particles of cloth", GH_ParamAccess.item, 0.0);
            // Lift
            pManager.AddNumberParameter("Lift", "lift", "Lift force applied to particles of cloth and inflatables",
              GH_ParamAccess.item, 0.0);
            // Relaxation Mode
            pManager.AddBooleanParameter("Relaxation Mode", "relaxation mode",
              "Set to true to apply RelaxationLocal Mode, will have slower convergence but more reliable. Set to false to apply RelaxationGlobal Mode, will be faster but could lead to errors in complexity.", GH_ParamAccess.item,
              true);
            // Relaxation Factor
            pManager.AddNumberParameter("Relaxation Factor", "relaxation factor",
              "Increase the convergence rate of solver, could be use to improve the efficiency of RelaxationLocal Mode while larger than 1.0, but it will lead to error when too larges.", GH_ParamAccess.item, 1.0);

            // loop through all params and set all of them optional
            for (int i = 0; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }

            #endregion
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem saveParamsBtn = Menu_AppendItem(menu, "Save Parameters", SaveParamsBtnClicked);
            saveParamsBtn.ToolTipText = "Save parameters as .xml file which could be import by \"Parameters Reader\" component for usuage next time";
        }

        private void SaveParamsBtnClicked(Object sender, EventArgs eventArgs)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Parameter XML files (*.xml)|*.xml";
            saveFileDialog.InitialDirectory = ".";
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    List<string> paramString = new List<string>();

                    paramString.Add("<?xml version=\"1.0\"?>\n<paramObj>");

                    #region write params into .xml
                    paramString.Add($"<{nameof(paramObj.Adhesion)}>{paramObj.Adhesion}</{nameof(paramObj.Adhesion)}>");
                    paramString.Add($"<{nameof(paramObj.Buoyancy)}>{paramObj.Buoyancy}</{nameof(paramObj.Buoyancy)}>");
                    paramString.Add($"<{nameof(paramObj.Cohesion)}>{paramObj.Cohesion}</{nameof(paramObj.Cohesion)}>");
                    paramString.Add($"<{nameof(paramObj.CollisionDistance)}>{paramObj.CollisionDistance}</{nameof(paramObj.CollisionDistance)}>");
                    paramString.Add($"<{nameof(paramObj.Damping)}>{paramObj.Damping}</{nameof(paramObj.Damping)}>");
                    paramString.Add($"<{nameof(paramObj.DiffuseBallistic)}>{paramObj.DiffuseBallistic}</{nameof(paramObj.DiffuseBallistic)}>");
                    paramString.Add($"<{nameof(paramObj.Dissipation)}>{paramObj.Dissipation}</{nameof(paramObj.Dissipation)}>");
                    paramString.Add($"<{nameof(paramObj.Drag)}>{paramObj.Drag}</{nameof(paramObj.Drag)}>");
                    paramString.Add($"<{nameof(paramObj.DynamicFriction)}>{paramObj.DynamicFriction}</{nameof(paramObj.DynamicFriction)}>");
                    paramString.Add($"<{nameof(paramObj.Fluid)}>{paramObj.Fluid}</{nameof(paramObj.Fluid)}>");
                    paramString.Add($"<{nameof(paramObj.FluidRestDistance)}>{paramObj.FluidRestDistance}</{nameof(paramObj.FluidRestDistance)}>");
                    paramString.Add($"<{nameof(paramObj.FreeSurfaceDrag)}>{paramObj.FreeSurfaceDrag}</{nameof(paramObj.FreeSurfaceDrag)}>");
                    paramString.Add($"<{nameof(paramObj.Lift)}>{paramObj.Lift}</{nameof(paramObj.Lift)}>");
                    paramString.Add($"<{nameof(paramObj.MaxAcceleration)}>{paramObj.MaxAcceleration}</{nameof(paramObj.MaxAcceleration)}>");
                    paramString.Add($"<{nameof(paramObj.MaxSpeed)}>{paramObj.MaxSpeed}</{nameof(paramObj.MaxSpeed)}>");
                    paramString.Add($"<{nameof(paramObj.ParticleCollisionMargin)}>{paramObj.ParticleCollisionMargin}</{nameof(paramObj.ParticleCollisionMargin)}>");
                    paramString.Add($"<{nameof(paramObj.ParticleFriction)}>{paramObj.ParticleFriction}</{nameof(paramObj.ParticleFriction)}>");
                    paramString.Add($"<{nameof(paramObj.PlasticCreep)}>{paramObj.PlasticCreep}</{nameof(paramObj.PlasticCreep)}>");
                    paramString.Add($"<{nameof(paramObj.PlasticThreshold)}>{paramObj.PlasticThreshold}</{nameof(paramObj.PlasticThreshold)}>");
                    paramString.Add($"<{nameof(paramObj.Radius)}>{paramObj.Radius}</{nameof(paramObj.Radius)}>");
                    paramString.Add($"<{nameof(paramObj.RelaxationFactor)}>{paramObj.RelaxationFactor}</{nameof(paramObj.RelaxationFactor)}>");
                    paramString.Add($"<{nameof(paramObj.RelaxationMode)}>{paramObj.RelaxationMode}</{nameof(paramObj.RelaxationMode)}>");
                    paramString.Add($"<{nameof(paramObj.Restitution)}>{paramObj.Restitution}</{nameof(paramObj.Restitution)}>");
                    paramString.Add($"<{nameof(paramObj.ShapeCollisionMargin)}>{paramObj.ShapeCollisionMargin}</{nameof(paramObj.ShapeCollisionMargin)}>");
                    paramString.Add($"<{nameof(paramObj.ShockPropagation)}>{paramObj.ShockPropagation}</{nameof(paramObj.ShockPropagation)}>");
                    paramString.Add($"<{nameof(paramObj.SleepThreshold)}>{paramObj.SleepThreshold}</{nameof(paramObj.SleepThreshold)}>");
                    paramString.Add($"<{nameof(paramObj.SolidPressure)}>{paramObj.SolidPressure}</{nameof(paramObj.SolidPressure)}>");
                    paramString.Add($"<{nameof(paramObj.SolidRestDistance)}>{paramObj.SolidRestDistance}</{nameof(paramObj.SolidRestDistance)}>");
                    paramString.Add($"<{nameof(paramObj.StaticFriction)}>{paramObj.StaticFriction}</{nameof(paramObj.StaticFriction)}>");
                    paramString.Add($"<{nameof(paramObj.SurfaceTension)}>{paramObj.SurfaceTension}</{nameof(paramObj.SurfaceTension)}>");
                    paramString.Add($"<{nameof(paramObj.Viscosity)}>{paramObj.Viscosity}</{nameof(paramObj.Viscosity)}>");
                    paramString.Add($"<{nameof(paramObj.WindX)}>{paramObj.WindX}</{nameof(paramObj.WindX)}>");
                    paramString.Add($"<{nameof(paramObj.WindY)}>{paramObj.WindY}</{nameof(paramObj.WindY)}>");
                    paramString.Add($"<{nameof(paramObj.WindZ)}>{paramObj.WindZ}</{nameof(paramObj.WindZ)}>");
                    //paramString.Add($"<{nameof()}>{}</{nameof()}>"); 
                    #endregion


                    System.IO.Stream stream = System.IO.File.Open(saveFileDialog.FileName, System.IO.FileMode.Create);
                    stream.Close();
                    System.IO.File.WriteAllLines(saveFileDialog.FileName, paramString);
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to save .xml file:\n" + e.Message);
                }
            }
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameters", "parameters", "Converted Parameters setting object for FleX engine",
              GH_ParamAccess.item);
        }

        /// <summary>
        /// global variable
        /// </summary>
        FlexParams paramObj = new FlexParams();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            paramObj = new FlexParams();

            // initialize coefficients
            // particle properties
            Vector3d ga = new Vector3d(0.0, 0.0, -9.807);
            double iRadius = 0.15;

            DA.GetData("Gravity Acceleration", ref ga);
            DA.GetData("Interaction Radius", ref iRadius);

            // collision
            double srd = 0.075;
            double frd = 0.075;
            double cd = 0.0;
            double pcm = 0.0;
            double scm = 0.0;
            double mxs = 0.0;
            double mxa = 0.0;

            DA.GetData("Solid Rest Distance", ref srd);
            DA.GetData("Fluid Rest Distance", ref frd);
            DA.GetData("Collision Distance", ref cd);
            DA.GetData("Particle Collision Margin", ref pcm);
            DA.GetData("Shape Collision Margin", ref scm);
            DA.GetData("Maximum Speed", ref mxs);
            DA.GetData("Maximum Acceleration", ref mxa);

            // friction
            double df = 0.0;
            double sf = 0.0;
            double pf = 0.0;
            double res = 0.0;
            double adh = 0.0;
            double pst = 0.0;
            double shp = 0.0;
            double dis = 0.0;
            double dam = 0.0;

            DA.GetData("Coefficient of Dynamic Friction", ref df);
            DA.GetData("Coefficient of Static Friction", ref sf);
            DA.GetData("Coefficient of Particle Friction", ref pf);
            DA.GetData("Restitution", ref res);
            DA.GetData("Adhesion", ref adh);
            DA.GetData("Particle Stop Threshold", ref pst);
            DA.GetData("Shock Propagation", ref shp);
            DA.GetData("Dissipation", ref dis);
            DA.GetData("Damping", ref dam);

            // fluid
            bool flu = true;
            double vis = 0.0;
            double coh = 0.0;
            double st = 0.0;
            double sp = 0.0;
            double fsd = 0.0;
            double buo = 0.0;

            DA.GetData("Is Fluid", ref flu);
            DA.GetData("Viscosity", ref vis);
            DA.GetData("Cohesion", ref coh);
            DA.GetData("Surface Tension", ref st);
            DA.GetData("Solid Pressure", ref sp);
            DA.GetData("Free Surface Drag", ref fsd);
            DA.GetData("Buoyancy", ref buo);

            // solid
            double sst = 0.0;
            double sc = 0.0;

            DA.GetData("Plastic Stop Threshold", ref sst);
            DA.GetData("Plastic Creep", ref sc);

            // cloth
            Vector3d wind = new Vector3d(0.0, 0.0, 0.0);
            double drag = 0.0;
            double lift = 0.0;
            bool rm = true;
            double rf = 1.0;

            DA.GetData("Wind", ref wind);
            DA.GetData("Drag", ref drag);
            DA.GetData("Lift", ref lift);
            DA.GetData("Relaxation Mode", ref rm);
            DA.GetData("Relaxation Factor", ref rf);

            // exception warning
            if (srd > iRadius)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Solid rest distance need not be larger than Interaction Radius.");
            }

            // particles properties
            paramObj.GravityX = (float)ga.X;
            paramObj.GravityY = (float)ga.Y;
            paramObj.GravityZ = (float)ga.Z;
            paramObj.Radius = (float)iRadius;

            // collision
            paramObj.SolidRestDistance = (float)srd;
            paramObj.FluidRestDistance = (float)frd;
            paramObj.CollisionDistance = (float)cd;
            paramObj.ParticleCollisionMargin = (float)pcm;
            paramObj.ShapeCollisionMargin = (float)scm;
            paramObj.MaxSpeed = (float)mxs;
            paramObj.MaxAcceleration = (float)mxa;

            // friction
            paramObj.DynamicFriction = (float)df;
            paramObj.StaticFriction = (float)sf;
            paramObj.ParticleFriction = (float)pf;
            paramObj.Restitution = (float)res;
            paramObj.Adhesion = (float)adh;
            paramObj.SleepThreshold = (float)pst;
            paramObj.ShockPropagation = (float)shp;
            paramObj.Dissipation = (float)dis;
            paramObj.Damping = (float)dam;

            // fluid
            paramObj.Fluid = flu;
            paramObj.Viscosity = (float)vis;
            paramObj.Cohesion = (float)coh;
            paramObj.SurfaceTension = (float)st;
            paramObj.SolidPressure = (float)sp;
            paramObj.FreeSurfaceDrag = (float)fsd;
            paramObj.Buoyancy = (float)buo;

            // solid
            paramObj.PlasticThreshold = (float)sst;
            paramObj.PlasticCreep = (float)sc;

            // cloth
            paramObj.WindX = (float)wind.X;
            paramObj.WindY = (float)wind.Y;
            paramObj.WindZ = (float)wind.Z;
            paramObj.Drag = (float)drag;
            paramObj.Lift = (float)lift;
            paramObj.RelaxationMode = rm ? 1 : 0;
            paramObj.RelaxationFactor = (float)rf;

            DA.SetData("Parameters", paramObj);
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
            get { return new Guid("bf130865-94d0-4671-ba5d-fc4875ebc5bb"); }
        }
    }
}