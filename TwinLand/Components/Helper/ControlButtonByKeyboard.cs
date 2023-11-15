using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino;
using Rhino.Geometry;

namespace TwinLand.Components.Helper
{
    public class ControlButtonByKeyboard : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the KeyboardController class.
        /// </summary>
        public ControlButtonByKeyboard()
          : base("Control Button By Keyboard", "control btn by keyboard",
              "", "Helper")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Button Component", "btn component", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Active", "active", "", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "log", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool active = true;
            DA.GetData("Active", ref active);

            Object btn_object = null;
            if (!DA.GetData("Button Component", ref btn_object)) return;
            System.Guid id = this.Params.Input[0].Sources[0].Attributes.DocObject.InstanceGuid;
            _btn = this.OnPingDocument().FindObject(id, true) as GH_ButtonObject;

            Instances.DocumentEditor.KeyDown -= new KeyEventHandler(KeyDwonEventHandler);
            Instances.DocumentEditor.KeyUp -= new KeyEventHandler(KeyUpEventHandler);
            if (active)
            {
                Instances.DocumentEditor.KeyDown += new KeyEventHandler(KeyDwonEventHandler);
                Instances.DocumentEditor.KeyUp += new KeyEventHandler(KeyUpEventHandler);
            }

            this.Message = $"Keyboard: {selectedKey}";

            DA.SetData("Log", _btn.ButtonDown ? "Button Down" : "Button Up");
        }

        /// <summary>
        /// Append additional menus
        /// </summary>
        /// <param name="menu"></param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem root = new ToolStripMenuItem("Select Hot Key");
            for (int i = 1; i <= 12; i++)
            {
                string keyName = $"F{i}";
                ToolStripMenuItem key = new ToolStripMenuItem(keyName);
                key.Tag = keyName;
                key.Checked = IsKeySelected(keyName);
                key.Click += KeyItemOnClicks;

                root.DropDownItems.Add(key);
            }

            menu.Items.Add(root);

            base.AppendAdditionalComponentMenuItems(menu);
        }

        private bool IsKeySelected(string keyName)
        {
            return keyName.Equals(selectedKey);
        }

        private void KeyItemOnClicks(object sender, EventArgs eventArgs)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;
            string label = item.Tag as string;
            if (IsKeySelected(label)) return;

            RecordUndoEvent("Select Key");
            selectedKey = label;
            KeysConverter kc = new KeysConverter();
            key = (Keys)kc.ConvertFromString(selectedKey);

            this.Message = $"Keyboard: {label}";
            ExpireSolution(true);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        GH_ButtonObject _btn = new GH_ButtonObject();
        Keys key = Keys.F7;
        string selectedKey = "F7";
        
        /// <summary>
        /// Additional methods
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void KeyDwonEventHandler(Object sender, KeyEventArgs e)
        {
            if (e.KeyCode != key || _btn.ButtonDown) return;
            _btn.ButtonDown = true;
            _btn.ExpireSolution(true);
        }
        
        void KeyUpEventHandler(Object sender, KeyEventArgs e)
        {
            if (e.KeyCode != key || !_btn.ButtonDown) return;
            _btn.ButtonDown = false;
            _btn.ExpireSolution(false);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("HotKey", selectedKey);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("HotKey"))
            {
                selectedKey = reader.GetString("HotKey");
                KeysConverter kc = new KeysConverter();
                key = (Keys)kc.ConvertFromString(selectedKey);
            }
            return base.Read(reader);
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
            get { return new Guid("8DD046E2-D7E1-43A1-AD93-6548264D4E4C"); }
        }
    }
}