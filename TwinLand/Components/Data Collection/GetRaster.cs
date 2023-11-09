
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;

using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using OSGeo.OGR;
using TwinLand;
using System.ComponentModel.Design.Serialization;
using System.CodeDom;
using System.Diagnostics;

namespace TwinLand
{
    public class GetRaster : TwinLandRasterPreviewComponent
    {
        //Class Constructor
        public GetRaster() : base("Get Raster", "Get Raster", "Get raster imagery from ArcGIS REST Services", "Data Collection")
        {

        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundary", "boundary", "Boundary of raster request to the REST server", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("Resolution", "resolution", "Resolution for raster images", GH_ParamAccess.item,1024);
            pManager.AddTextParameter("FolderPath", "folderPath", "Target folder for saving raster images", GH_ParamAccess.item, Path.GetTempPath());
            pManager.AddTextParameter("FileName", "fileName", "File name of downloaded images", GH_ParamAccess.item, "restRaster");
            //pManager.AddTextParameter("ServiceURL", "serviceURL", "ArcGIS REST Service website to query. Use the component \nmenu item \"Create REST Raster Source List\" for some examples.", GH_ParamAccess.item);

            // optional(preset) input
            pManager.AddTextParameter("Spatial Reference System", "customSRS", "Customize your Spatial Reference System by standard SRS label", GH_ParamAccess.item, "WGS84");
            pManager.AddTextParameter("ImageType", "imageType", "Set image type for the request to the REST server", GH_ParamAccess.item, "jpg");
            pManager.AddBooleanParameter("Start Download", "start download", "Start to download raster image data from the REST server", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("ImagePath", "ImagePath", "Full path of downloaded raster images", GH_ParamAccess.tree);
            pManager.AddCurveParameter("ImageExtent", "imageExtent", "The original frame of the downloaded raster image before projection", GH_ParamAccess.tree);
            pManager.AddTextParameter("REST_query", "REST_query", "REST query string of the REST request", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> boundary = new List<Curve>();
            DA.GetDataList<Curve>("Boundary", boundary);

            //int resolution = -1;
            //DA.GetData<int>("Resolution", ref resolution);

            int resolution = resolutionValue;

            string folderPath = string.Empty;
            DA.GetData<string>("FolderPath", ref folderPath);
            if (Helper.isWindows && !folderPath.EndsWith(@"\"))
            {
                folderPath += @"\";
            }
            else if (!Helper.isWindows && !folderPath.EndsWith(@"/"))
            {
                folderPath += @"/";
            }

            string fileName = string.Empty;
            DA.GetData<string>("FileName", ref fileName);

            //string serviceURL = string.Empty;
            //DA.GetData<string>("ServiceURL", ref serviceURL);
            //if (serviceURL.EndsWith(@"/")) { serviceURL = serviceURL + "export?"; }
            string serviceURL = rasterURL;

            bool run = false;
            DA.GetData<bool>("Start Download", ref run);

            string SRS_code = string.Empty;
            DA.GetData<string>("Spatial Reference System", ref SRS_code);

            string imageType = string.Empty;
            DA.GetData<string>("ImageType", ref imageType);

            ///GDAL setup
            RESTful.GdalConfiguration.ConfigureOgr();

            ///TODO: implement SetCRS here.
            ///Option to set CRS here to user-defined.  Needs a SetCRS global variable.
            //string SRS_code = "EPSG:4326";

            OSGeo.OSR.SpatialReference customSRS = new OSGeo.OSR.SpatialReference("");
            customSRS.SetFromUserInput(SRS_code);
            int userSRSInt = Int16.Parse(customSRS.GetAuthorityCode(null));

            ///Set transform from input spatial reference to Rhino spatial reference
            OSGeo.OSR.SpatialReference rhinoSRS = new OSGeo.OSR.SpatialReference("");
            rhinoSRS.SetWellKnownGeogCS("WGS84");

            ///This transform moves and scales the points required in going from customSRS to XYZ and vice versa
            Transform userSRSToModelTransform = TwinLand.Convert.GetUserSRSToModelTransform(customSRS);
            Transform modelToUserSRSTransform = TwinLand.Convert.GetModelToUserSRSTransform(customSRS);


            GH_Structure<GH_String> mapList = new GH_Structure<GH_String>();
            GH_Structure<GH_String> mapquery = new GH_Structure<GH_String>();
            GH_Structure<GH_Rectangle> imgFrame = new GH_Structure<GH_Rectangle>();

            FileInfo file = new FileInfo(folderPath);
            file.Directory.Create();

            string size = string.Empty;
            if (resolution != 0)
            {
                size = "&size=" + resolution + "%2C" + resolution;
            }

            for (int i = 0; i < boundary.Count; i++)
            {

                GH_Path path = new GH_Path(i);

                ///Get image frame for given boundary
                BoundingBox imageBox = boundary[i].GetBoundingBox(false);
                imageBox.Transform(modelToUserSRSTransform);

                ///Make sure to have a rect for output
                Rectangle3d rect = BBoxToRect(imageBox);

                ///Query the REST service
                string restquery = serviceURL +
                  ///legacy method for creating bounding box string
                  "bbox=" + imageBox.Min.X + "%2C" + imageBox.Min.Y + "%2C" + imageBox.Max.X + "%2C" + imageBox.Max.Y +
                  "&bboxSR=" + userSRSInt +
                  size + //"&layers=&layerdefs=" +
                  "&imageSR=" + userSRSInt + //"&transparent=false&dpi=&time=&layerTimeOptions=" +
                  "&format=" + imageType;// +
                                         //"&f=json";
                string restqueryJSON = restquery + "&f=json";
                string restqueryImage = restquery + "&f=image";

                mapquery.Append(new GH_String(restqueryImage), path);

                string result = string.Empty;

                ///Get extent of image from arcgis rest service as JSON
                result = TwinLand.Convert.HttpToJson(restqueryJSON);
                JObject jObj = JsonConvert.DeserializeObject<JObject>(result);
                if (!jObj.ContainsKey("href"))
                {
                    restqueryJSON = restqueryJSON.Replace("export?", "exportImage?");
                    restqueryImage = restqueryImage.Replace("export?", "exportImage?");
                    mapquery.RemovePath(path);
                    mapquery.Append(new GH_String(restqueryImage), path);
                    result = TwinLand.Convert.HttpToJson(restqueryJSON);
                    jObj = JsonConvert.DeserializeObject<JObject>(result);
                }

                // Prepare rectangle
                Point3d extMin = new Point3d((double)jObj["extent"]["xmin"], (double)jObj["extent"]["ymin"], 0);
                Point3d extMax = new Point3d((double)jObj["extent"]["xmax"], (double)jObj["extent"]["ymax"], 0);
                rect = new Rectangle3d(Plane.WorldXY, extMin, extMax);
                rect.Transform(userSRSToModelTransform);

                if (run)
                {
                    ///Download image from source
                    ///Catch if JSON query throws an error
                    string imageQueryJSON = jObj["href"].ToString();
                    using (WebClient webC = new WebClient())
                    {
                        try
                        {
                            if (!String.IsNullOrEmpty(imageQueryJSON))
                            {
                                webC.DownloadFile(imageQueryJSON, folderPath + fileName + "_" + i + "." + imageType);
                                webC.Dispose();
                            }
                            else
                            {
                                webC.DownloadFile(restqueryImage, folderPath + fileName + "_" + i + "." + imageType);
                                webC.Dispose();
                            }

                        }
                        catch (WebException e)
                        {
                            webC.DownloadFile(restqueryImage, folderPath + fileName + "_" + i + "." + imageType);
                            webC.Dispose();
                        }
                    }
                }

                var bitmapPath = folderPath + fileName + "_" + i + "." + imageType;
                mapList.Append(new GH_String(bitmapPath), path);

                imgFrame.Append(new GH_Rectangle(rect), path);
                AddPreviewItem(bitmapPath, rect);
            }

            this.Message = GetMessage(dynamicMessage);

            DA.SetDataTree(0, mapList);
            DA.SetDataTree(1, imgFrame);
            DA.SetDataTree(2, mapquery);
        }

        /// <summary>
        /// Adds to the context menu an option to create a pre-populated list of common REST Raster sources
        /// </summary>
        /// <param name="menu"></param>
        /// https://discourse.mcneel.com/t/generated-valuelist-not-working/79406/6?u=hypar
        //public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        //{
        //    var rasterSourcesJson = sourceJson["REST Raster"].Select(x => x["source"]).Distinct();
        //    List<string> rasterSources = rasterSourcesJson.Values<string>().ToList();
        //    foreach (var src in rasterSourcesJson)
        //    {
        //        ToolStripMenuItem root = GH_DocumentObject.Menu_AppendItem(menu, "Create " + src.ToString() + " Source List", CreateRasterList);
        //        root.ToolTipText = "Click this to create a pre-populated list of some " + src.ToString() + " sources.";
        //        base.AppendAdditionalMenuItems(menu);
        //    }
        //}

        ///// <summary>
        ///// Creates a value list pre-populated with possible accent colors and adds it to the Grasshopper Document, located near the component pivot.
        ///// </summary>
        ///// <param name="sender">The sender.</param>
        ///// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        //private void CreateRasterList(object sender, System.EventArgs e)
        //{
        //    string source = sender.ToString();
        //    source = source.Replace("Create ", "");
        //    source = source.Replace(" Source List", "");

        //    GH_DocumentIO docIO = new GH_DocumentIO();
        //    docIO.Document = new GH_Document();

        //    ///Initialize object
        //    GH_ValueList vl = new GH_ValueList();

        //    ///Clear default contents
        //    vl.ListItems.Clear();

        //    foreach (var service in sourceJson["REST Raster"])
        //    {
        //        if (service["source"].ToString() == source)
        //        {
        //            GH_ValueListItem vi = new GH_ValueListItem(service["service"].ToString(), String.Format("\"{0}\"", service["url"].ToString()));
        //            vl.ListItems.Add(vi);
        //        }
        //    }

        //    ///Set component nickname
        //    vl.NickName = source;

        //    ///Get active GH doc
        //    GH_Document doc = OnPingDocument();
        //    if (docIO.Document == null) return;

        //    ///Place the object
        //    docIO.Document.AddObject(vl, false, 1);

        //    ///Get the pivot of the "serviceURL" param
        //    PointF currPivot = Params.Input[4].Attributes.Pivot;

        //    ///Set the pivot of the new object
        //    vl.Attributes.Pivot = new PointF(currPivot.X - 400, currPivot.Y - 11);

        //    docIO.Document.SelectAll();
        //    docIO.Document.ExpireSolution();
        //    docIO.Document.MutateAllIds();
        //    IEnumerable<IGH_DocumentObject> objs = docIO.Document.Objects;
        //    doc.DeselectAll();
        //    doc.UndoUtil.RecordAddObjectEvent("Create REST Raster Source List", objs);
        //    doc.MergeDocument(docIO.Document);
        //}

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            // create raster service menu
            ToolStripMenuItem root_rasterService = new ToolStripMenuItem("Select Raster Service");

            foreach (var rasterService in sourceJson["REST Raster"])
            {
                string serviceString = rasterService["service"].ToString();

                ToolStripMenuItem serviceItem = new ToolStripMenuItem(serviceString);
                serviceItem.Tag = serviceString;
                serviceItem.Checked = IsRasterServiceSelected(serviceString);
                serviceItem.ToolTipText = rasterService["description"].ToString();
                serviceItem.Click += ServiceItemOnClick;

                root_rasterService.DropDownItems.Add(serviceItem);
            }

            // create resolution menu
            ToolStripMenuItem root_resolution = new ToolStripMenuItem("Select Raster Resolution");

            foreach (var resolution in sourceJson["Raster Resolution"])
            {
                string resolutionString = resolution["level"].ToString();
                string resolutionValue = resolution["resolution"].ToString();

                ToolStripMenuItem resolutionItem = new ToolStripMenuItem(resolutionString);
                resolutionItem.Tag = resolutionString;
                resolutionItem.Checked = IsResolutionSelected(resolutionValue);
                resolutionItem.ToolTipText = resolution["description"].ToString();
                resolutionItem.Click += ResolutionOnClick;

                root_resolution.DropDownItems.Add(resolutionItem);
            }

            // append items onto menu in component
            menu.Items.Add(root_rasterService);
            menu.Items.Add(root_resolution);

            base.AppendAdditionalComponentMenuItems(menu);
        }

        private bool IsRasterServiceSelected(string serviceString)
        {
            return serviceString.Equals(rasterSource);
        }

        private bool IsResolutionSelected(string selectedValue)
        {
            return selectedValue.Equals(resolutionValue.ToString());
        }


        private void ServiceItemOnClick(object sender, EventArgs eventArgs)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;
            string label = (string)item.Tag;
            if (IsRasterServiceSelected(label)) return;

            RecordUndoEvent("Raster Source");
            RecordUndoEvent("Raster URL");

            rasterSource = label;
            rasterURL = sourceJson["REST Raster"].SelectToken("[?(@.service == '" + rasterSource + "')].url").ToString();
            dynamicMessage[0] = label;
            Message = GetMessage(dynamicMessage);

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

            resolutionLevel = label;
            string resolutionValueString = sourceJson["Raster Resolution"].SelectToken("[?(@.level == '" + resolutionLevel + "')].resolution").ToString();
            resolutionValue = Int32.Parse(resolutionValueString);
            dynamicMessage[1] = resolutionLevel;
            Message = GetMessage(dynamicMessage);

            ExpireSolution(true);
        }

        private string GetMessage(string[] messageInfos)
        {
            string message = "Raster Service: " + messageInfos[0] + ", " + "Raster Resolution: " + messageInfos[1];

            return message;
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        private string sourceList = TwinLand.Convert.GetEndpoints();
        private JObject sourceJson = JObject.Parse(TwinLand.Convert.GetEndpoints());

        // default raster service selection
        private string rasterSource = JObject.Parse(TwinLand.Convert.GetEndpoints())["REST Raster"][0]["service"].ToString();
        private string rasterURL = JObject.Parse(TwinLand.Convert.GetEndpoints())["REST Raster"][0]["url"].ToString();

        // default raster resolution selection
        private static string resolutionLevel = "max";
        private int resolutionValue = 1024;

        // dynamic message in component
        private string[] dynamicMessage = new string[2] { JObject.Parse(TwinLand.Convert.GetEndpoints())["REST Raster"][0]["service"].ToString(), resolutionLevel };

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("Source_Raster", rasterSource);
            writer.SetString("Resolution", resolutionLevel);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("Source_Raster"))
            {
                rasterSource = reader.GetString("Source_Raster");
            }
            if (reader.ItemExists("Resolution"))
            {
                resolutionLevel = reader.GetString("Resolution");
            }
            return base.Read(reader);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.T_icon;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{480f1a1e-db8a-4c46-851f-0e87df4ca605}"); }
        }
    }
}
