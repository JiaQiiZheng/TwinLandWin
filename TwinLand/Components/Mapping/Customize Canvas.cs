using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Display;
using Rhino.Geometry;

namespace TwinLand.Components.Mapping
{
    public class Customize_Canvas : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Customize_Canvas class.
        /// </summary>
        public Customize_Canvas()
          : base("Customize Canvas", "customize canvas",
              "Customize grasshopper document canvas for layout use", "Mapping")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("canvas_grid", "canvas_grid", "", GH_ParamAccess.item);
            pManager.AddColourParameter("canvas_back", "canvas_back", "", GH_ParamAccess.item);
            pManager.AddColourParameter("canvas_edge", "canvas_edge", "", GH_ParamAccess.item);
            pManager.AddColourParameter("canvas_shade", "canvas_shade", "", GH_ParamAccess.item);
            pManager.AddColourParameter("panel_back", "panel_back", "", GH_ParamAccess.item);
            pManager.AddColourParameter("wire_default", "wire_default", "", GH_ParamAccess.item);
            pManager.AddColourParameter("wire_empty", "wire_empty", "", GH_ParamAccess.item);
            pManager.AddColourParameter("group_back", "group_back", "", GH_ParamAccess.item);

            pManager.AddBooleanParameter("Apply", "apply", "", GH_ParamAccess.item);

            for (int i = 0; i < pManager.ParamCount-1; i++)
            {
                pManager[i].Optional = true;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("GH_Skin Configuration", "configuration", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool apply = false;
            Color canvas_grid = Color.White;
            Color canvas_back = Color.White;
            Color canvas_edge = Color.White;
            Color canvas_shade = Color.White;
            Color panel_back = Color.Black;
            Color wire_default = Color.Gray;
            Color wire_empty = Color.Gray;
            Color group_back = Color.LightGray;

            if (!DA.GetData("Apply", ref apply)) { return; }
            DA.GetData("canvas_grid", ref canvas_grid);
            DA.GetData("canvas_back", ref canvas_back);
            DA.GetData("canvas_edge", ref canvas_edge);
            DA.GetData("canvas_shade", ref canvas_shade);
            DA.GetData("panel_back", ref panel_back);
            DA.GetData("wire_default", ref wire_default);
            DA.GetData("wire_empty", ref wire_empty);
            DA.GetData("group_back", ref group_back);

            if (apply)
            {
                // update new value in existed GUI file
                GH_Skin.canvas_grid = canvas_grid;
                GH_Skin.canvas_back = canvas_back;
                GH_Skin.canvas_edge = canvas_edge;
                GH_Skin.canvas_shade = canvas_shade;
                GH_Skin.panel_back = panel_back;
                GH_Skin.wire_default = wire_default;
                GH_Skin.wire_empty = wire_empty;
                GH_Skin.group_back = group_back;
            }
            else
            {
                // reload the default GUI.xml
                GH_Skin.LoadSkin();
            }

            List<String> config_params = new List<string>();
            DA.SetDataList("GH_Skin Configuration", config_params);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripDropDownItem save_GUI_button = Menu_AppendItem(menu, "Save GUI as .xml", Save_GUI_btn_Clicked);
            save_GUI_button.ToolTipText = "Save GH_Skin configuration as GUI .xml file into grasshopper_gui.xml settings database.";
        }
        
        private void Save_GUI_btn_Clicked(Object sender, EventArgs eventArgs)
        {
            GH_Skin.SaveSkin();
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
                return Properties.Resources.T_icon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C88F8E3C-BEA2-448A-B5E2-3D809B4C334E"); }
        }
    }
}