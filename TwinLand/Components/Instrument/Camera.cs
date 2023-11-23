using System;
using System.Collections.Generic;
using Rhino.Display;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino;
using System.Diagnostics;
using Rhino.DocObjects;
using System.Windows.Forms;
using Grasshopper;

namespace TwinLand.Components.Instrument
{
    public class Camera : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Camera class.
        /// </summary>
        public Camera()
          : base("Camera", "camera",
              "", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Camera Location", "camera location", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Camera Target", "target", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Lens Length", "lens length", "", GH_ParamAccess.item, 24);
            pManager.AddIntegerParameter("Perspective", "perspective", "0-isometric view, 1-perspective view, 2-2d-perspective view", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Viewport Update", "viewport update", "", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("Cruise Mode", "cruise mode", "", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Rotation Speed", "rotation speed", "", GH_ParamAccess.item, 0.1);
            pManager.AddBooleanParameter("Cruise Active", "cruise active", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Active", "active", "", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Camera", "camera", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d location = new Point3d();
            Point3d target = new Point3d();
            Vector3d direction = new Vector3d();
            double lensLength = 35.0;
            int perspective = 1;
            bool viewportUpdate = true;
            int cruiseMode = 1;
            double rotationSpeed = 0.1;
            bool cruiseActive = false;
            bool active = true;

            if (!DA.GetData("Camera Location", ref location)) return;
            if (!DA.GetData("Camera Target", ref target)) return;
            DA.GetData("Lens Length", ref lensLength);
            DA.GetData("Perspective", ref perspective);
            DA.GetData("Viewport Update", ref viewportUpdate);
            DA.GetData("Cruise Mode", ref cruiseMode);
            DA.GetData("Rotation Speed", ref rotationSpeed);
            DA.GetData("Cruise Active", ref cruiseActive);
            DA.GetData("Active", ref active);

            RhinoViewport vp = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            distance = location.DistanceTo(target);
            direction = new Vector3d(target.X - location.X, target.Y - location.Y, target.Z - location.Z);

            if (active)
            {
                Instances.DocumentEditor.KeyDown -= new KeyEventHandler(KeyDownEventHandler);
                Instances.DocumentEditor.KeyDown += new KeyEventHandler(KeyDownEventHandler);

                if (perspective == 0)
                {
                    vp.ChangeToParallelProjection(true);
                    BoundingBox bb = BoundingBox.Unset;
                    if (viewportUpdate)
                    {
                        bb = new BoundingBox(target.X - distance, target.Y - distance, target.Z - distance, target.X + distance, target.Y + distance, target.Z + distance);
                        vp.SetCameraLocation(location, true);
                        vp.SetCameraDirection(direction, true);
                    }
                    else
                    {
                        bb = new BoundingBox(target.X - zoomDistance, target.Y - zoomDistance, target.Z - zoomDistance, target.X + zoomDistance, target.Y + zoomDistance, target.Z + zoomDistance);
                    }

                    vp.ZoomBoundingBox(bb);
                }
                else if (perspective == 1)
                {
                    vp.ChangeToPerspectiveProjection(true, lensLength);
                    if (viewportUpdate)
                    {
                        vp.SetCameraLocation(location, true);
                        vp.SetCameraDirection(direction, true);
                        vp.Camera35mmLensLength = lensLength;

                        BoundingBox bb = BoundingBox.Unset;
                        bb = new BoundingBox(target.X - distance, target.Y - distance, target.Z - distance, target.X + distance, target.Y + distance, target.Z + distance);
                        vp.ZoomBoundingBox(bb);
                    }
                }

                if (cruiseActive)
                {
                    if (cruiseMode == 1)
                    {
                        vp.KeyboardRotate(true, RhinoMath.ToRadians(rotationSpeed));
                        ExpireSolution(true);
                    }
                }
            }

            vp.GetCameraFrame(out frame);
            DA.SetData("Camera", frame);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        Plane frame = Plane.Unset;
        double distance;
        double zoomDistance = 10000.0;

        /// <summary>
        /// Keys
        /// </summary>
        void KeyDownEventHandler(Object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyData == Keys.Oemplus)
            {
                zoomDistance -= (distance / 10);
                ExpireSolution(true);
            }
            else if (eventArgs.KeyData == Keys.OemMinus)
            {
                zoomDistance += (distance / 10);
                ExpireSolution(true);
            }
            else
            {
                return;
            }

            ExpireSolution(true);
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
                return Properties.Resources.TL_Engine;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("CFABB175-63A0-4EE2-8378-6D88B7EDC072"); }
        }
    }
}