using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace TwinLand.Components.Instrument
{
    public class Olmsted : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Olmsted class.
        /// </summary>
        public Olmsted()
          : base("Olmsted", "olmsted",
              "", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Height", "height", "", GH_ParamAccess.item, 1800);
            pManager.AddNumberParameter("Moving Speed", "moving speed", "", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Turning Speed", "turning speed", "degree per second", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("First-Person Perspective", "fp perspective", "", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Walking", "walking", "", GH_ParamAccess.item, false);
            pManager.AddGeometryParameter("Earth", "earth", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Preview Radius", "preview radius", "", GH_ParamAccess.item, 2000);
            pManager.AddBooleanParameter("Control Active", "control active", "", GH_ParamAccess.item, true);

            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Location", "location", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Sight Direction", "sight front", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Sight Target", "sight target", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryBase> earth = new List<GeometryBase>();
            bool fp = true;

            DA.GetData("Height", ref height);
            DA.GetData("Moving Speed", ref movingSpeed);
            DA.GetData("Turning Speed", ref turningSpeed);
            DA.GetData("First-Person Perspective", ref fp);
            DA.GetData("Walking", ref walking);
            DA.GetData("Preview Radius", ref radius);
            DA.GetData("Control Active", ref active);

            if (!active) return;

            direction = vp.CameraDirection;
            front = new Vector3d(direction.X, direction.Y, 0);

            left = front;
            left.Rotate(Math.PI / 2, Vector3d.ZAxis);

            right = front;
            right.Rotate(-Math.PI / 2, Vector3d.ZAxis);

            back = -front;

            Instances.DocumentEditor.KeyDown -= new KeyEventHandler(KeyDownEventHandler);
            Instances.DocumentEditor.KeyDown += new KeyEventHandler(KeyDownEventHandler);

            if (walking)
            {
                // Try to project camera onto the earth

                Point3d[] curProjectedLocation = new Point3d[1] { new Point3d(vp.CameraLocation.X, vp.CameraLocation.Y, 0) };
                DA.GetDataList("Earth", earth);

                if (earth != null && earth.Count > 0)
                {
                    List<Point3d> projectedCollection = new List<Point3d>();

                    foreach (var geo in earth)
                    {
                        if (geo.GetType().Name == "Mesh")
                        {
                            Point3d[] projected = Intersection.ProjectPointsToMeshes(new Mesh[1] { geo as Mesh }, curProjectedLocation, Vector3d.ZAxis, tl);

                            if (projected.Length > 0)
                            {
                                foreach (Point3d pt in projected)
                                {
                                    projectedCollection.Add(pt);
                                }
                            }
                        }
                        else if (geo.GetType().Name == "Brep")
                        {
                            Point3d[] projected = Intersection.ProjectPointsToBreps(new Brep[1] { geo as Brep }, curProjectedLocation, Vector3d.ZAxis, tl);

                            if (projected.Length > 0)
                            {
                                foreach (Point3d pt in projected)
                                {
                                    projectedCollection.Add(pt);
                                }
                            }
                        }
                    }

                    // Set new camera location
                    if (projectedCollection.Count > 0)
                    {
                        Point3d[] candidates = projectedCollection.ToArray();
                        Array.Sort(candidates, new ZComparer());
                        candidates[0] += new Vector3d(0, 0, height);
                        vp.SetCameraLocation(candidates[0], updateTarget);
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Failed to land on the earth.");
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No earth input");
                }
            }

            Point3d target = vp.CameraLocation + vp.CameraDirection * radius;

            DA.SetData("Location", vp.CameraLocation);
            DA.SetData("Sight Direction", vp.CameraDirection);
            DA.SetData("Sight Target", target);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        Keys w = Keys.W;
        Keys s = Keys.S;
        Keys a = Keys.A;
        Keys d = Keys.D;
        Keys q = Keys.Q;
        Keys e = Keys.E;
        Keys r = Keys.R;
        Keys z = Keys.Z;
        Keys f = Keys.F;
        Keys c = Keys.C;
        Keys v = Keys.V;

        // Combo keys
        string shift = Keys.Shift.ToString();

        bool updateTarget = true;
        double radius = 2000;
        double height = 1800.0;
        double movingSpeed = 1.0;
        double turningSpeed = 1.0;
        bool walking = false;
        bool active = true;

        double tl = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
        RhinoViewport vp = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
        double accelerateFactor = 3.0;

        Plane frame = Plane.Unset;

        Vector3d direction = Vector3d.Unset;
        Vector3d front = Vector3d.Unset;
        Vector3d left = Vector3d.Unset;
        Vector3d right = Vector3d.Unset;
        Vector3d back = Vector3d.Unset;
        Vector3d lift = Vector3d.ZAxis;
        Vector3d sink = -Vector3d.ZAxis;

        /// <summary>
        /// Events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void KeyDownEventHandler(Object sender, KeyEventArgs eventArgs)
        {
            if (!active) return;

            if (eventArgs.KeyData.ToString().Contains(shift))
            {
                movingSpeed *= accelerateFactor;
                turningSpeed *= accelerateFactor;
            }

            // Moving
            if (eventArgs.KeyCode == w)
            {
                front.Unitize();
                vp.SetCameraLocation(vp.CameraLocation + front * movingSpeed, updateTarget);
            }
            else if (eventArgs.KeyCode == s)
            {
                back.Unitize();
                vp.SetCameraLocation(vp.CameraLocation + back * movingSpeed, updateTarget);
            }
            else if(eventArgs.KeyCode == a)
            {
                left.Unitize();
                vp.SetCameraLocation(vp.CameraLocation + left * movingSpeed, updateTarget);
            }
            else if(eventArgs.KeyCode == d)
            {
                right.Unitize();
                vp.SetCameraLocation(vp.CameraLocation + right * movingSpeed, updateTarget);
            }

            // Turning
            else if(eventArgs.KeyCode == q)
            {
                direction.Rotate(RhinoMath.ToRadians(turningSpeed), Vector3d.ZAxis);
                vp.SetCameraDirection(direction, updateTarget);
            }
            else if(eventArgs.KeyCode == e)
            {
                direction.Rotate(RhinoMath.ToRadians(-turningSpeed), Vector3d.ZAxis);
                vp.SetCameraDirection(direction, updateTarget);
            }

            // Rising and sinking
            else if(eventArgs.KeyCode == r && !walking)
            {
                vp.SetCameraLocation(vp.CameraLocation + lift * movingSpeed, updateTarget);
            }
            else if(eventArgs.KeyCode == z && !walking)
            {
                vp.SetCameraLocation(vp.CameraLocation + sink * movingSpeed, updateTarget);
            }

            // Camera up and down
            else if(eventArgs.KeyCode == f)
            {
                vp.GetCameraFrame(out frame);
                Vector3d cur = vp.CameraDirection;
                cur.Rotate(RhinoMath.ToRadians(turningSpeed), frame.XAxis);
                vp.SetCameraDirection(cur, updateTarget);
            }
            else if(eventArgs.KeyCode == c)
            {
                vp.GetCameraFrame(out frame);
                Vector3d cur = vp.CameraDirection;
                cur.Rotate(-RhinoMath.ToRadians(turningSpeed), frame.XAxis);
                vp.SetCameraDirection(cur, updateTarget);
            }

            else
            {
                return;
            }
            
            vp.SetCameraTarget(vp.CameraLocation + direction * radius, false);
            vp.SetClippingPlanes(new BoundingBox(new Point3d(vp.CameraLocation.X - radius, vp.CameraLocation.Y - radius, vp.CameraLocation.Z - radius), new Point3d(vp.CameraLocation.X - radius, vp.CameraLocation.Y - radius, vp.CameraLocation.Z - radius)));

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
            get { return new Guid("B5991339-DCE6-4D37-B590-13542F3FE5C4"); }
        }
    }
}