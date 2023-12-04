﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument.Charactor
{
    public class CharactorConstructor : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Charactor Constructor class.
        /// </summary>
        public CharactorConstructor()
          : base("Charactor Constructor", "charactor constructor",
              "", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "name", "", GH_ParamAccess.item, "Olmsted");
            pManager.AddMeshParameter("Body", "body", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Moving Speed", "moving speed", "", GH_ParamAccess.item, 100.0);
            pManager.AddNumberParameter("Turning Speed", "turning speed", "", GH_ParamAccess.item, 1.0);
            pManager.AddVectorParameter("Orientation", "orientation", "", GH_ParamAccess.item, Vector3d.XAxis);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Charactor", "charactor", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = string.Empty;
            List<Mesh> bodies = new List<Mesh>();
            double movingSpeed = 100.0;
            double turningSpeed = 1.0;
            Vector3d orientation = Vector3d.XAxis;

            DA.GetData("Name", ref name);
            if (!DA.GetDataList("Body", bodies)) return;
            DA.GetData("Moving Speed", ref movingSpeed);
            DA.GetData("Turning Speed", ref turningSpeed);
            DA.GetData("Orientation", ref orientation);

            charactor = new CharactorObject(name, bodies, movingSpeed, turningSpeed, orientation);

            DA.SetData("Charactor", charactor);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        CharactorObject charactor = null;

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
            get { return new Guid("BAE8D32A-24EC-4328-86F9-B1BBF6D3357D"); }
        }
    }
}