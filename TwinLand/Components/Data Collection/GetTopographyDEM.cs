using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;

using Grasshopper;
using GH_IO;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Geometry;
using System.Security;
using System.Diagnostics;
using OSGeo.OGR;
using OSGeo.GDAL;

namespace TwinLand
{
    public class GetTopographyDEM : TwinLandComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GetTopographyDEM()
          : base("Get Topography (DEM)", "Get Topography (DEM)",
            "Get Topography based on DEM data from services ", "Data Collection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundary", "boundary", "The download area of DEM file", GH_ParamAccess.list);
            pManager.AddTextParameter("TargetFolder", "targetFolder",
              "The target folder used to place the downloaded DEM file", GH_ParamAccess.item, Path.GetTempPath());
            pManager.AddTextParameter("FileName", "fileName", "The file name of downloaded DEM file", GH_ParamAccess.item);
            pManager.AddTextParameter("Spatial Reference System", "customSRS", "Customize your Spatial Reference System by standard SRS label", GH_ParamAccess.item, "WGS84");
            pManager.AddBooleanParameter("Start Download", "start download", "Start to download the DEM file from the server", GH_ParamAccess.item, false);

            pManager[2].Optional = true;

            Message = GetMessage(dynamicMessage); ;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("DEM_FilePath", "DEM", "The file path of downloaded DEM file", GH_ParamAccess.tree);
            pManager.AddTextParameter("TopoQuery", "TopoQuery", "The response query from DEM file download process",
              GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> boundary = new List<Curve>();
            DA.GetDataList<Curve>("Boundary", boundary);

            int resolution = resolutionValue;
            string resolutionLabel = resolutionLevel;

            string folderPath = string.Empty;
            DA.GetData("TargetFolder", ref folderPath);
            if (Helper.isWindows && !folderPath.EndsWith(@"\"))
            {
                folderPath += @"\";
            }
            else if (!Helper.isWindows && !folderPath.EndsWith(@"/"))
            {
                folderPath += @"/";
            }

            string fileName = string.Empty;
            DA.GetData("FileName", ref fileName);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = source_DEM;
            }

            string SRS_code = String.Empty;
            DA.GetData<String>("Spatial Reference System", ref SRS_code);

            bool run = false;
            DA.GetData("Start Download", ref run);

            //// configure download DEM size
            //string size = string.Empty;
            //string sizeLabel = string.Empty;
            //if(resolution != 0 && resolutionLabel != string.Empty)
            //{
            //    size = "&size=" + resolution + "%2C" + resolution;
            //}

            GH_Structure<GH_String> demList = new GH_Structure<GH_String>();
            GH_Structure<GH_String> demQuery = new GH_Structure<GH_String>();

            ///GDAL setup
            RESTful.GdalConfiguration.ConfigureOgr();

            // Initial SRS instance from GDAL
            OSGeo.OSR.SpatialReference customSRS = new OSGeo.OSR.SpatialReference("");
            customSRS.SetFromUserInput(SRS_code);
            int userSRSInt = Int16.Parse(customSRS.GetAuthorityCode(null));

            for (int i = 0; i < boundary.Count; i++)
            {
                GH_Path path = new GH_Path(i);

                // get DEM file based on input valid boundary
                if (!boundary[i].GetBoundingBox(true).IsValid)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid boundary exist.");
                    return;
                }

                double distance = 200 * Rhino.RhinoMath.UnitScale(UnitSystem.Meters, RhinoDoc.ActiveDoc.ModelUnitSystem);
                Curve offsetBoundary = boundary[i].Offset(Rhino.Geometry.Plane.WorldXY, distance, 1, CurveOffsetCornerStyle.Sharp)[0];

                Point3d min = TwinLand.Convert.XYZToWGS(offsetBoundary.GetBoundingBox(true).Min);
                Point3d max = TwinLand.Convert.XYZToWGS(offsetBoundary.GetBoundingBox(true).Max);

                double left = min.X;
                double bottom = min.Y;
                double right = max.X;
                double top = max.Y;

                // Complete query url with resolution setting
                string topoQuery = String.Empty;
                if (source_DEM.Equals("USGS"))
                {
                    topoQuery = String.Format(dem_url, left, bottom, right, top, resolution, resolution, userSRSInt);
                }
                else if (source_DEM.Equals("GMRT"))
                {
                    topoQuery = String.Format(dem_url, left, bottom, right, top, resolutionLabel);
                }

                // prepare fileFullPath for download or load purpose
                string fileFullPath = $"{folderPath}{fileName}_{i}.tif";

                if (run)
                {
                    WebClient wb = new WebClient();
                    wb.DownloadFile(topoQuery, fileFullPath);
                    wb.Dispose();
                }

                demList.Append(new GH_String(fileFullPath), path);
                demQuery.Append(new GH_String(topoQuery), path);
            }

            DA.SetDataTree(0, demList);
            DA.SetDataTree(1, demQuery);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            if (string.IsNullOrEmpty(sourceList_DEM))
            {
                sourceList_DEM = TwinLand.Convert.GetEndpoints();
            }

            //// This only use single level of list
            //foreach (var service in sourceJson["REST Topo"])
            //{
            //    string sName = service["service"].ToString();

            //    ToolStripMenuItem serviceItem = new ToolStripMenuItem(sName);
            //    serviceItem.Tag = sName;
            //    serviceItem.Checked = IsServiceSelected(sName);
            //    serviceItem.ToolTipText = service["description"].ToString();
            //    serviceItem.Click += ServiceItemOnClicks;

            //    menu.Items.Add(serviceItem);
            //}

            // Create DEM service menu
            ToolStripMenuItem root_service = new ToolStripMenuItem("Select Service");

            foreach (var source in sourceJson["REST Topo"])
            {
                string serviceName = source["service"].ToString();
                ToolStripMenuItem serviceItem = new ToolStripMenuItem(serviceName);
                serviceItem.Tag = serviceName;
                serviceItem.Checked = IsServiceSelected(serviceName);
                serviceItem.ToolTipText = source["description"].ToString();
                serviceItem.Click += ServiceItemOnClicks;

                root_service.DropDownItems.Add(serviceItem);
            }

            // Create resolution menu
            ToolStripMenuItem root_resolution = new ToolStripMenuItem("Select DEM Resolution");

            foreach (var resolution in sourceJson["Raster Resolution"])
            {
                string resolutionLevel = resolution["level"].ToString();
                string resolutionValue = resolution["resolution"].ToString();

                ToolStripMenuItem resolutionItem = new ToolStripMenuItem(resolutionLevel);
                resolutionItem.Tag = resolutionLevel;
                resolutionItem.Checked = IsResolutionSelected(resolutionValue);
                resolutionItem.ToolTipText = resolution["description"].ToString();
                resolutionItem.Click += ResolutionOnClick;

                root_resolution.DropDownItems.Add(resolutionItem);
            }

            // Append items onto menu in component
            menu.Items.Add(root_service);
            menu.Items.Add(root_resolution);

            base.AppendAdditionalComponentMenuItems(menu);
        }

        /// <summary>
        /// additional menu items
        /// </summary>
        /// <param name="serviceString"></param>
        /// <returns></returns>
        private bool IsServiceSelected(string serviceString)
        {
            return serviceString.Equals(source_DEM);
        }

        private bool IsResolutionSelected(string selectedValue)
        {
            return selectedValue.Equals(resolutionValue.ToString());
        }

        private void ServiceItemOnClicks(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;

            string label = (string)item.Tag;
            if (IsServiceSelected(label)) { return; }

            RecordUndoEvent("source_DEM");

            source_DEM = label;
            //TODO. study more about JSONPath expression.
            dem_url = JObject.Parse(SourceList_DEM)["REST Topo"].SelectToken("[?(@.service == '" + Source_DEM + "')].url").ToString();
            dynamicMessage[0] = label;
            Message = GetMessage(dynamicMessage); ;

            ExpireSolution(true);
        }
        
        private void ResolutionOnClick(object sender, EventArgs eventArgs)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;
            string label = (string)item.Tag;
            if (IsResolutionSelected(label)) return;

            RecordUndoEvent("Resolution Level");
            RecordUndoEvent("Resolution Value");

            string resolutionValueString = sourceJson["Raster Resolution"].SelectToken("[?(@.level == '" + resolutionLevel + "')].resolution").ToString();
            resolutionLevel = label;
            resolutionValue = Int32.Parse(resolutionValueString);
            dynamicMessage[1] = resolutionLevel;
            Message = GetMessage(dynamicMessage);

            ExpireSolution(true);
        }

        private string GetMessage(string[] messageInfo)
        {
            string message = "DEM service: " + messageInfo[0] + ", " + "DEM Resolution: " + messageInfo[1];
            return message;
        }

        /// <summary>
        /// Dynamic Variables
        /// </summary>
        private string sourceList_DEM = TwinLand.Convert.GetEndpoints();
        private JObject sourceJson = JObject.Parse(TwinLand.Convert.GetEndpoints());
        private string source_DEM = JObject.Parse(TwinLand.Convert.GetEndpoints())["REST Topo"][0]["service"].ToString();
        private string dem_url = JObject.Parse(TwinLand.Convert.GetEndpoints())["REST Topo"][0]["url"].ToString();

        private string resolutionLevel = "med";
        private int resolutionValue = 512;

        // dynamic message in component
        private string[] dynamicMessage = new string[2] { JObject.Parse(TwinLand.Convert.GetEndpoints())["REST Topo"][0]["service"].ToString(), "med" };

        public string SourceList_DEM
        {
            get { return sourceList_DEM; }
            set { sourceList_DEM = value; }
        }

        public string Source_DEM
        {
            get { return source_DEM; }
            set
            {
                source_DEM = value;
                Message = GetMessage(dynamicMessage);
            }
        }

        public string DEM_URL
        {
            get { return dem_url; }
            set { dem_url = value; }
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetString("Source_DEM", Source_DEM);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            Source_DEM = reader.GetString("Source_DEM");
            return base.Read(reader);
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
                return Properties.Resources.T_icon;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4147d23d-db2f-4692-bce2-190aea4d3f59"); }
        }
    }
}