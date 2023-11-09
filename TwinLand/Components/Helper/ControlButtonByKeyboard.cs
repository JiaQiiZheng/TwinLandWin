﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
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

            this.Message = $"Keyboard: {key}";

            DA.SetData("Log", _btn.ButtonDown ? "Button Down" : "Button Up");
        }
        
        /// <summary>
        /// Dynamic variables
        /// </summary>
        GH_ButtonObject _btn = new GH_ButtonObject();
        Keys key = Keys.F7;
        
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