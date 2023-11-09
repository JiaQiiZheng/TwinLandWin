using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using FlexCLI;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Configuration
{
    public class Parameters_Collection : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Parameters_Collection class.
        /// </summary>
        public Parameters_Collection()
          : base("Parameters Collection", "param collection",
              "Collect all parameters", "Configuration")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameter Global", "param global", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Parameter Collision", "param collision", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Parameter Friction", "param friction", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Parameter Fluid", "param fluid", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Parameter Solid", "param solid", "", GH_ParamAccess.item);

            for (int i = 0; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameters", "parameters", "Converted Parameters setting object for FleX engine",
              GH_ParamAccess.item);
            pManager.AddTextParameter("Parameters Logger", "parameters logger", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            paramObj = new FlexParams();
            List<ParamCombo> paramComboList = new List<ParamCombo>();
            List<string> paramLog = new List<string>();

            // Parameter Global
            FlexParams paramGlobal = new FlexParams();
            Vector3d ga = new Vector3d(0.0, 0.0, -9.807);
            double iRadius = 0.15;
            double mxs = 0.0;
            double mxa = 0.0;
            double pst = 0.0;
            double sst = 0.0;
            bool flu = true;
            bool rm = true;
            double rf = 1.0;

            DA.GetData("Parameter Global", ref paramGlobal);

            ga = new Vector3d(paramGlobal.GravityX, paramGlobal.GravityY, paramGlobal.GravityZ);
            iRadius = paramGlobal.Radius;
            mxs = paramGlobal.MaxSpeed;
            mxa = paramGlobal.MaxAcceleration;
            pst = paramGlobal.SleepThreshold;
            sst = paramGlobal.PlasticThreshold;
            flu = paramGlobal.Fluid;
            rm = paramGlobal.RelaxationMode == 1 ? true : false;
            rf = paramGlobal.RelaxationFactor;

            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("Parameter Global"));
            paramComboList.Add(new ParamCombo("\r\n"));


            paramComboList.Add(new ParamCombo("Gravity Acceleration", ga));
            paramComboList.Add(new ParamCombo("Interaction Radius", iRadius));
            paramComboList.Add(new ParamCombo("Maximum Speed", mxs));
            paramComboList.Add(new ParamCombo("Maximum Acceleration", mxa));
            paramComboList.Add(new ParamCombo("Particle Stop Threshold", pst));

            // Collision
            FlexParams paramCollision = new FlexParams();

            DA.GetData("Parameter Collision", ref paramCollision);

            double srd = 0.075;
            double frd = 0.075;
            double cd = 0.0;
            double pcm = 0.0;
            double scm = 0.0;

            srd = paramCollision.SolidRestDistance;
            frd = paramCollision.FluidRestDistance;
            cd = paramCollision.CollisionDistance;
            pcm = paramCollision.ParticleCollisionMargin;
            scm = paramCollision.ShapeCollisionMargin;

            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("Parameter Collision"));
            paramComboList.Add(new ParamCombo("\r\n"));

            paramComboList.Add(new ParamCombo("Solid Rest Distance", srd));
            paramComboList.Add(new ParamCombo("Fluid Rest Distance", frd));
            paramComboList.Add(new ParamCombo("Collision Distance", cd));
            paramComboList.Add(new ParamCombo("Particle Collision Margin", pcm));
            paramComboList.Add(new ParamCombo("Shape Collision Margin", scm));


            // Friction
            FlexParams paramFriction = new FlexParams();

            DA.GetData("Parameter Friction", ref paramFriction);

            double df = 0.0;
            double sf = 0.0;
            double pf = 0.0;
            double res = 0.0;
            double adh = 0.0;
            double shp = 0.0;
            double dis = 0.0;
            double dam = 0.0;

            df = paramFriction.DynamicFriction;
            sf = paramFriction.StaticFriction;
            pf = paramFriction.ParticleFriction;
            res = paramFriction.Restitution;
            adh = paramFriction.Adhesion;
            shp = paramFriction.ShockPropagation;
            dis = paramFriction.Dissipation;
            dam = paramFriction.Damping;

            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("Parameter Friction"));
            paramComboList.Add(new ParamCombo("\r\n"));

            paramComboList.Add(new ParamCombo("Coefficient of Dynamic Friction", df));
            paramComboList.Add(new ParamCombo("Coefficient of Static Friction", sf));
            paramComboList.Add(new ParamCombo("Coefficient of Particle Friction", pf));
            paramComboList.Add(new ParamCombo("Restitution", res));
            paramComboList.Add(new ParamCombo("Adhesion", adh));
            paramComboList.Add(new ParamCombo("Shock Propagation", shp));
            paramComboList.Add(new ParamCombo("Dissipation", dis));
            paramComboList.Add(new ParamCombo("Damping", dam));


            // Fluid
            FlexParams paramFluid = new FlexParams();

            DA.GetData("Parameter Fluid", ref paramFluid);

            double vis = 0.0;
            double coh = 0.0;
            double st = 0.0;
            double fsd = 0.0;
            double buo = 0.0;

            vis = paramFluid.Viscosity;
            coh = paramFluid.Cohesion;
            st = paramFluid.SurfaceTension;
            fsd = paramFluid.FreeSurfaceDrag;
            buo = paramFluid.Buoyancy;

            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("Parameter Fluid"));
            paramComboList.Add(new ParamCombo("\r\n"));

            paramComboList.Add(new ParamCombo("Viscosity", vis));
            paramComboList.Add(new ParamCombo("Cohesion", coh));
            paramComboList.Add(new ParamCombo("SurfaceTension", st));
            paramComboList.Add(new ParamCombo("FreeSurfaceDrag", fsd));
            paramComboList.Add(new ParamCombo("Buoyancy", buo));

            // Solid
            FlexParams paramSolid = new FlexParams();

            DA.GetData("Parameter Solid", ref paramSolid);

            double sc = 0.0;
            double sp = 0.0;

            sc = paramSolid.PlasticCreep;
            sp = paramSolid.SolidPressure;

            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("\r\n"));
            paramComboList.Add(new ParamCombo("Parameter Solid"));
            paramComboList.Add(new ParamCombo("\r\n"));

            paramComboList.Add(new ParamCombo("PlasticCreep", sc));
            paramComboList.Add(new ParamCombo("SolidPressure", sp));

            //// Cloth
            //Vector3d wind = new Vector3d(0.0, 0.0, 0.0);
            //double drag = 0.0;
            //double lift = 0.0;

            //DA.GetData("Wind", ref wind);
            //DA.GetData("Drag", ref drag);
            //DA.GetData("Lift", ref lift);


            // exception warning
            if (srd > iRadius)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Solid rest distance need not be larger than Interaction Radius.");
            }

            // Global
            paramObj.GravityX = (float)ga.X;
            paramObj.GravityY = (float)ga.Y;
            paramObj.GravityZ = (float)ga.Z;
            paramObj.Radius = (float)iRadius;
            paramObj.MaxSpeed = (float)mxs;
            paramObj.MaxAcceleration = (float)mxa;
            paramObj.SleepThreshold = (float)pst;
            paramObj.PlasticThreshold = (float)sst;
            paramObj.Fluid = flu;
            paramObj.RelaxationMode = rm ? 1 : 0;
            paramObj.RelaxationFactor = (float)rf;


            // Collision
            paramObj.SolidRestDistance = (float)srd;
            paramObj.FluidRestDistance = (float)frd;
            paramObj.CollisionDistance = (float)cd;
            paramObj.ParticleCollisionMargin = (float)pcm;
            paramObj.ShapeCollisionMargin = (float)scm;


            // Friction
            paramObj.DynamicFriction = (float)df;
            paramObj.StaticFriction = (float)sf;
            paramObj.ParticleFriction = (float)pf;
            paramObj.Restitution = (float)res;
            paramObj.Adhesion = (float)adh;
            paramObj.ShockPropagation = (float)shp;
            paramObj.Dissipation = (float)dis;
            paramObj.Damping = (float)dam;

            // Fluid
            paramObj.Viscosity = (float)vis;
            paramObj.Cohesion = (float)coh;
            paramObj.SurfaceTension = (float)st;
            paramObj.FreeSurfaceDrag = (float)fsd;
            paramObj.Buoyancy = (float)buo;

            // Solid
            paramObj.PlasticCreep = (float)sc;
            paramObj.SolidPressure = (float)sp;

            //// cloth
            //paramObj.WindX = (float)wind.X;
            //paramObj.WindY = (float)wind.Y;
            //paramObj.WindZ = (float)wind.Z;
            //paramObj.Drag = (float)drag;
            //paramObj.Lift = (float)lift;


            // append params combo into logger
            foreach (var combo in paramComboList)
            {
                paramLog.Add(GetParamsString(combo));
            }

            DA.SetData("Parameters", paramObj);
            DA.SetDataList(1, paramLog);
        }

        /// <summary>
        /// Additional methods
        /// </summary>
        /// <param name="menu"></param>
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

            DialogResult res = saveFileDialog.ShowDialog();

            if (res == DialogResult.OK)
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

                    paramString.Add("</paramObj>");


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
        /// Dynamic variable
        /// </summary>
        FlexParams paramObj = new FlexParams();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>

        private string GetParamsString(ParamCombo pc)
        {
            string str = String.Empty;
            if (pc.paramName != null && pc.paramValue != null && pc.paramType != null)
            {
                str = $"{pc.paramName} = {pc.paramValue} {new string('-', 20)} Pamameter Type: {pc.paramType.Split('.')[pc.paramType.Split('.').Length - 1]}";
            }
            else
            {
                str = pc.paramGroup;
            }

            return str;
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
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F5B7CED7-2C25-48D2-956D-7BD5AE2810D4"); }
        }
    }
}