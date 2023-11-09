using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using TwinLand.Utils;
using System.Runtime.CompilerServices;
using System.Drawing;
using Rhino.Render.DataSources;

namespace TwinLand.Components.Helper
{
    public class ControlTriggerByKeyboard : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ControlTriggerByKeyboard class.
        /// </summary>
        public ControlTriggerByKeyboard()
          : base("Control Trigger By Keyboard", "control trigger by keyboard",
              "", "Helper")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Active", "active", "", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Guid", "guid", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool active = true;
            DA.GetData("Active", ref active);

            List<Guid> guids = new List<Guid>();
            List<IGH_DocumentObject> selected = null;
            if (this.Message == "Selected!")
            {
                selected = this.OnPingDocument().SelectedObjects();
                foreach (var obj in selected)
                {
                    if (obj.GetType() == typeof(GH_Timer))
                    {
                        Guid id = obj.InstanceGuid;
                        guids.Add(id);
                        _timer = (GH_Timer)this.OnPingDocument().FindObject(id, false);

                        // Apply hot key
                        Instances.DocumentEditor.KeyDown -= new KeyEventHandler(KeyDownEventHandler);
                        if (active)
                        {
                            Instances.DocumentEditor.KeyDown += new KeyEventHandler(KeyDownEventHandler);
                        }

                    }
                    else
                    {
                        this.Message = String.Empty;
                    }
                }
            }

            this.Message = $"Keyboard: {selectedKey}";

            DA.SetDataList("Guid", guids);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        GH_Timer _timer = new GH_Timer();
        Keys key = Keys.F9;
        string selectedKey = "F9";

        /// <summary>
        /// Custom attributes - Select Button
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new Attributes_Custom(this); ;
        }

        void KeyDownEventHandler(Object sender, KeyEventArgs e)
        {
            if (e.KeyCode != key) return;
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
            get { return new Guid("8C309E13-D9C9-4C59-AA9B-E4046D90E02E"); }
        }
    }
}