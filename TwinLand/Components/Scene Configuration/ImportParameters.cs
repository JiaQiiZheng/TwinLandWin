using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using FlexCLI;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.FleX_Configuration
{
    public class ImportParameters : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ImportParameters class.
        /// </summary>
        public ImportParameters()
          : base("Import Parameters", "import parameters",
              "Import parameters from .xml file", "Configuration")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "file path", "File full path of .xml file", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameters", "parameters", "Parameters object for Engine", GH_ParamAccess.item);
            pManager.AddTextParameter("Parameters Logger", "parameters logger", "", GH_ParamAccess.list);
        }

        bool isDefaultFile = false;

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripItem default_params = Menu_AppendItem(menu, "Generate File: Default Parameters", GenerateBtnClicked);
            default_params.ToolTipText = "Generate a .xml file containing default parameters";
        }

        private void GenerateBtnClicked(object sender, EventArgs eventArgs)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.FileName = "default parameters.xml";
            saveFile.InitialDirectory = ".";
            saveFile.Filter = "Parameter XML files (*.xml)|*.xml|All files (*.*)|*.*";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string defaultString = "<?xml version=\"1.0\"?><params><GravityX>0.0</GravityX><GravityY>0.0</GravityY><GravityZ>-9.81</GravityZ><WindX>0.0</WindX><WindY>0.0</ ><WindZ>0.0</WindZ><Radius>0.15</Radius><Viscosity>0.0</Viscosity><DynamicFriction>0.0</DynamicFriction><StaticFriction>0.0</StaticFriction><ParticleFriction>0.0</ParticleFriction><FreeSurfaceDrag>0.0</FreeSurfaceDrag><Drag>0.0</Drag><Lift>0.0</Lift><FluidRestDistance>0.1</FluidRestDistance><SolidRestDistance>0.15</SolidRestDistance><Dissipation>0.0</Dissipation><Damping>0.0</Damping><ParticleCollisionMargin>0.075</ParticleCollisionMargin><ShapeCollisionMargin>0.075</ShapeCollisionMargin><CollisionDistance>0.075</CollisionDistance><PlasticThreshold>0.0</PlasticThreshold><PlasticCreep>0.0</PlasticCreep><Fluid>true</Fluid><SleepThreshold>0.0</SleepThreshold><ShockPropagation>0.0</ShockPropagation><Restitution>0.0</Restitution><MaxSpeed>3.402823466e+38</MaxSpeed><MaxAcceleration>100.0</MaxAcceleration><RelaxationMode>1</RelaxationMode><RelaxationFactor>1.0</RelaxationFactor><SolidPressure>1.0</SolidPressure><Adhesion>0.0</Adhesion><Cohesion>0.025</Cohesion><SurfaceTension>0.0</SurfaceTension><Buoyancy>1.0</Buoyancy></params>";

                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(defaultString);

                    Stream stream = System.IO.File.Open(saveFile.FileName, System.IO.FileMode.Create);
                    xdoc.Save(stream);
                    path = saveFile.FileName;
                    stream.Close();
                    isDefaultFile = true;
                    ExpireSolution(true);
                }
                catch (Exception e)
                {

                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to save .xml file:\n" + e.ToString());
                }
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FlexParams param = new FlexParams();
            List<string> paramComboList = new List<string>();

            if (!isDefaultFile)
            {

                DA.GetData("File Path", ref path);

            }
            else
            {
                isDefaultFile = false;
            }

            XmlDocument doc = new XmlDocument();
            string root = string.Empty;

            if (String.IsNullOrEmpty(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to collect data or invalid path");
                return;
            }

            // if path is only fileName, the file is generated default parameter.xml, add a root path of the file in front it in order to laod with a full path
            if (!path.Contains("/") && !path.Contains(@"\"))
            {
                root = this.OnPingDocument().FilePath;
                path = root.Substring(0, root.LastIndexOf(@"\") + 1) + path;
            }

            doc.Load(path);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                #region get params from .xml file
                if (node.Name == "Adhesion") param.Adhesion = float.Parse(node.InnerText);
                else if (node.Name == "AnisotropyMax") param.AnisotropyMax = float.Parse(node.InnerText);
                else if (node.Name == "AnisotropyMin")
                    param.AnisotropyMin = float.Parse(node.InnerText);

                else if (node.Name == "AnisotropyScale")
                    param.AnisotropyScale = float.Parse(node.InnerText);

                else if (node.Name == "Buoyancy")
                    param.Buoyancy = float.Parse(node.InnerText);

                else if (node.Name == "Cohesion")
                    param.Cohesion = float.Parse(node.InnerText);

                else if (node.Name == "CollisionDistance")
                    param.CollisionDistance = float.Parse(node.InnerText);

                else if (node.Name == "Damping")
                    param.Damping = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseBallistic")
                    param.DiffuseBallistic = int.Parse(node.InnerText);

                else if (node.Name == "DiffuseBuoyancy")
                    param.DiffuseBuoyancy = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseDrag")
                    param.DiffuseDrag = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseLifetime")
                    param.DiffuseLifetime = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseSortAxisX")
                    param.DiffuseSortAxisX = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseSortAxisY")
                    param.DiffuseSortAxisY = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseSortAxisZ")
                    param.DiffuseSortAxisZ = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseThreshold")
                    param.DiffuseThreshold = float.Parse(node.InnerText);

                else if (node.Name == "Dissipation")
                    param.Dissipation = float.Parse(node.InnerText);

                else if (node.Name == "Drag")
                    param.Drag = float.Parse(node.InnerText);

                else if (node.Name == "DynamicFriction")
                    param.DynamicFriction = float.Parse(node.InnerText);

                else if (node.Name == "Fluid")
                    param.Fluid = bool.Parse(node.InnerText);

                else if (node.Name == "FluidRestDistance")
                    param.FluidRestDistance = float.Parse(node.InnerText);

                else if (node.Name == "FreeSurfaceDrag")
                    param.FreeSurfaceDrag = float.Parse(node.InnerText);

                else if (node.Name == "GravityX")
                    param.GravityX = float.Parse(node.InnerText);

                else if (node.Name == "GravityY")
                    param.GravityY = float.Parse(node.InnerText);

                else if (node.Name == "GravityZ")
                    param.GravityZ = float.Parse(node.InnerText);

                else if (node.Name == "Lift")
                    param.Lift = float.Parse(node.InnerText);

                else if (node.Name == "MaxAcceleration")
                    param.MaxAcceleration = float.Parse(node.InnerText);

                else if (node.Name == "MaxSpeed")
                    param.MaxSpeed = float.Parse(node.InnerText);

                else if (node.Name == "NumIterations")
                    param.NumIterations = int.Parse(node.InnerText);

                else if (node.Name == "NumPlanes")
                    param.NumPlanes = int.Parse(node.InnerText);

                else if (node.Name == "ParticleCollisionMargin")
                    param.ParticleCollisionMargin = float.Parse(node.InnerText);

                else if (node.Name == "ParticleFriction")
                    param.ParticleFriction = float.Parse(node.InnerText);

                else if (node.Name == "PlasticCreep")
                    param.PlasticCreep = float.Parse(node.InnerText);

                else if (node.Name == "PlasticThreshold")
                    param.PlasticThreshold = float.Parse(node.InnerText);

                else if (node.Name == "Radius")
                    param.Radius = float.Parse(node.InnerText);

                else if (node.Name == "RelaxationFactor")
                    param.RelaxationFactor = float.Parse(node.InnerText);

                else if (node.Name == "RelaxationMode")
                    param.RelaxationMode = int.Parse(node.InnerText);

                else if (node.Name == "Restitution")
                    param.Restitution = float.Parse(node.InnerText);

                else if (node.Name == "ShapeCollisionMargin")
                    param.ShapeCollisionMargin = float.Parse(node.InnerText);

                else if (node.Name == "ShockPropagation")
                    param.ShockPropagation = float.Parse(node.InnerText);

                else if (node.Name == "SleepThreshold")
                    param.SleepThreshold = float.Parse(node.InnerText);

                else if (node.Name == "Smoothing")
                    param.Smoothing = float.Parse(node.InnerText);

                else if (node.Name == "SolidPressure")
                    param.SolidPressure = float.Parse(node.InnerText);

                else if (node.Name == "SolidRestDistance")
                    param.SolidRestDistance = float.Parse(node.InnerText);

                else if (node.Name == "StaticFriction")
                    param.StaticFriction = float.Parse(node.InnerText);

                else if (node.Name == "SurfaceTension")
                    param.SurfaceTension = float.Parse(node.InnerText);

                else if (node.Name == "Viscosity")
                    param.Viscosity = float.Parse(node.InnerText);

                else if (node.Name == "VorticityConfinement")
                    param.VorticityConfinement = float.Parse(node.InnerText);

                else if (node.Name == "WindX")
                    param.WindX = float.Parse(node.InnerText);

                else if (node.Name == "WindY")
                    param.WindY = float.Parse(node.InnerText);

                else if (node.Name == "WindZ")
                    param.WindZ = float.Parse(node.InnerText);

                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Param couldn't be identified: " + node.Name);
                #endregion

                // append param values into parameters logger
                paramComboList.Add($"{node.Name} = {node.InnerText}");
            }

            this.Message = path.Split('\\')[path.Split('\\').Length - 1];

            DA.SetData("Parameters", param);
            DA.SetDataList(1, paramComboList);
        }

        /// <summary>
        /// global variables
        /// </summary>
        string path = string.Empty;

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
            get { return new Guid("25C54D69-3A11-4D48-A22C-2403660B2818"); }
        }
    }
}