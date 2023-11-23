using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Display;
using Rhino;
using Rhino.Geometry;
using System.Windows.Forms;
using Grasshopper;
using System.Diagnostics;
using Rhino.Geometry.Intersect;
using TwinLand.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.ComponentModel;
using System.Timers;

namespace TwinLand.Components.Instrument
{
    public class Controller : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the CharactorController class.
        /// </summary>
        public Controller()
          : base("Controller", "controller",
              "", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Birth Point", "birth pt", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Charactor", "charactor", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Instrument", "instrument", "", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Earth", "earth", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Accelerate", "accelerate", "", GH_ParamAccess.item, 3.0);
            pManager.AddBooleanParameter("Camera Follow", "camera follow", "", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Viewport Update", "viewport update", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Active", "active", "", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Reset", "reset", "", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Charactor", "charactor", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Instrument", "instrument", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Camera Location", "camera location", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Camera Direction", "camera direction", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RhinoViewport vp = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;

            Point3d birthLocation = Point3d.Unset;
            double accelerate = 3.0;
            List<Mesh> bodies = new List<Mesh>();
            List<GeometryBase> earth = new List<GeometryBase>();
            bool cameraFollow = true;
            bool reset = false;

            DA.GetData("Birth Point", ref birthLocation);
            if (!DA.GetData("Charactor", ref charactor)) return;
            DA.GetData("Instrument", ref instrument);
            if (!DA.GetDataList("Earth", earth)) return;
            DA.GetData("Accelerate", ref accelerate);
            DA.GetData("Camera Follow", ref cameraFollow);
            DA.GetData("Viewport Update", ref viewportUpdate);
            DA.GetData("Active", ref active);
            DA.GetData("Reset", ref reset);

            // Initial charactor's properties
            if (bodies.Count == 0)
            {
                bodies = charactor.Bodies;
            }
            name = charactor.Name;
            movingSpeed = charactor.MovingSpeed;
            turningSpeed = charactor.TurningSpeed;
            _moving = accelerate * movingSpeed;
            _turning = accelerate * turningSpeed;

            // Initial instrument's properties

            // Initial location and direction when first put charactor into model space
            if (!active) return;
            if (reset)
            {
                showStep = 0;
                bodyLocation = birthLocation;
                bodyDirection = Vector3d.VectorAngle(Vector3d.XAxis, charactor.Orientation);
                cameraHeight = 6000;
                cameraDistance = 18000;
                DA.GetData("Charactor", ref charactor);
            }

            // Reorientation
            front = new Vector3d(Math.Cos(bodyDirection), Math.Sin(bodyDirection), 0);

            left = front;
            left.Rotate(Math.PI / 2, Vector3d.ZAxis);

            right = front;
            right.Rotate(-Math.PI / 2, Vector3d.ZAxis);

            back = -front;

            Instances.DocumentEditor.KeyDown -= new KeyEventHandler(KeyDownEventHandler);
            Instances.DocumentEditor.KeyDown += new KeyEventHandler(KeyDownEventHandler);

            // Project onto the earth
            Point3d[] curProjectedLocation = new Point3d[1] { bodyLocation };
            if (earth.Count > 0)
            {
                List<Point3d> projectedCollection = new List<Point3d>();

                foreach (var geo in earth)
                {
                    if (geo.GetType().Name == "Mesh")
                    {
                        Point3d[] projected = Intersection.ProjectPointsToMeshes(new Mesh[1] { geo as Mesh }, curProjectedLocation, Vector3d.ZAxis, tl);
                        if (projected.Length > 0)
                        {
                            projectedCollection.AddRange(projected);
                        }
                    }
                    else if (geo.GetType().Name == "Brep")
                    {
                        Point3d[] projected = Intersection.ProjectPointsToBreps(new Brep[1] { geo as Brep }, curProjectedLocation, Vector3d.ZAxis, tl);

                        if (projected.Length > 0)
                        {
                            projectedCollection.AddRange(projected);
                        }
                    }
                    else if (geo.GetType().Name == "Plane")
                    {
                        double elevation = geo.GetBoundingBox(true).Min.Z;
                        projectedCollection.Add(new Point3d(bodyLocation.X, bodyLocation.Y, elevation));
                    }
                }

                if (projectedCollection.Count > 0)
                {
                    // Update body location onto earth
                    Point3d[] candidates = projectedCollection.ToArray();
                    Array.Sort(candidates, new ZComparer());
                    bodyLocation = candidates[0];
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

            // Show a series actions of body movement
            int bodyCount = bodies.Count;
            int index = bodyCount != 0 && showStep >= bodyCount ? showStep % bodyCount : showStep;

            // Make a copy from bodies
            Mesh use = new Mesh();
            use.Append(bodies[index]);

            Point3d min = use.GetBoundingBox(true).Min;
            Point3d max = use.GetBoundingBox(true).Max;
            double bodyHeight = max.Z - min.Z;

            // Get eyes location
            BoundingBox bb = use.GetBoundingBox(true);
            Point3d foot = bb.Center - new Vector3d(0, 0, bodyHeight / 2);

            // Tracking body movement
            Transform translate = Transform.Translation(bodyLocation - foot);
            Transform rotation = Transform.Rotation(bodyDirection, foot);
            use.Transform(translate * rotation);

            // Update output CharactorObject
            charactor.BodyStatus = use;
            charactor.BodyLocation = bodyLocation;
            charactor.Orientation = front;

            // Update output InstrumentObject

            if (instrument.PlanarPointEmitter.Active)
            {
                Point3d emitPt = bodyLocation + new Vector3d(0, 0, bodyHeight);
                instrument.PlanarPointEmitter.Plane = new Plane(emitPt, front);
            }
            else if (!instrument.PlanarPointEmitter.Active)
            {
                // Delete current emit boundary
                instrument.PlanarPointEmitter.EmittingBoundary = new Rectangle3d();
            }

            showStep += 1;

            // Update camera
            Point3d camera = bodyLocation;

            Vector3d move = Vector3d.Unset;
            if (cameraFollow)
            {
                move = back * cameraDistance + new Vector3d(0, 0, cameraHeight);
                camera += move;

                if (viewportUpdate)
                {
                    // No smooth
                    vp.SetCameraLocation(camera, false);
                    vp.SetCameraDirection(-move, true);
                }
            }

            // Update instrument status
            DA.GetData("Instrument", ref instrument);

            DA.SetData("Charactor", charactor);
            DA.SetData("Instrument", instrument);
            DA.SetData("Camera Location", camera);
            DA.SetData("Camera Direction", -move);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        bool active = true;

        // Charactor properties
        CharactorObject charactor = null;
        InstrumentObject instrument = null;
        string name = "Olmsted";
        double movingSpeed = 100;
        double turningSpeed = 1;
        double _moving = 1.0;
        double _turning = 1.0;
        bool emitting = false;

        int showStep = 0;
        double tl = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
        System.Timers.Timer timer = new System.Timers.Timer();
        bool viewportUpdate = false;
        bool instrumentToggle = false;

        /// <summary>
        /// Keys
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
        string shift = Keys.Shift.ToString();

        Point3d bodyLocation;
        double bodyDirection;
        double cameraHeight = 6000;
        double cameraDistance = 18000;
        Vector3d front = Vector3d.Unset;
        Vector3d left = Vector3d.Unset;
        Vector3d right = Vector3d.Unset;
        Vector3d back = Vector3d.Unset;

        /// <summary>
        /// Events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void KeyDownEventHandler(Object sender, KeyEventArgs eventArgs)
        {
            // Combo keys
            string keyStr = eventArgs.KeyData.ToString();
            if (keyStr.Contains(shift))
            {
                movingSpeed = _moving;
                turningSpeed = _turning;
                if (instrumentToggle)
                {
                    instrument.PlanarPointEmitter.Trigger(instrument.PlanarPointEmitter.Plane);
                }
            }
            else if (!keyStr.Contains(shift) && !emitting)
            {
                // Not emit any point when no shift pressed
                instrument.PlanarPointEmitter.Points.Clear();
                instrument.PlanarPointEmitter.Velocities.Clear();
            }

            // Moving
            if (eventArgs.KeyCode == w)
            {
                front.Unitize();
                bodyLocation += (front * movingSpeed);
            }
            else if (eventArgs.KeyCode == s)
            {
                back.Unitize();
                bodyLocation += (back * movingSpeed);
            }
            else if (eventArgs.KeyCode == a)
            {
                left.Unitize();
                bodyLocation += (left * movingSpeed);

                //// Smooth turning 90 degree
                //TurningSmooth(smoothStep, Math.PI / 2);

                bodyDirection += Math.PI / 2;
            }
            else if (eventArgs.KeyCode == d)
            {
                right.Unitize();
                bodyLocation += (right * movingSpeed);

                //// Smooth turning -90 degree
                //TurningSmooth(smoothStep, -Math.PI / 2);

                bodyDirection -= Math.PI / 2;
            }

            // Turning
            else if (eventArgs.KeyCode == q)
            {
                bodyDirection += RhinoMath.ToRadians(turningSpeed);
            }
            else if (eventArgs.KeyCode == e)
            {
                bodyDirection -= RhinoMath.ToRadians(turningSpeed);
            }

            // Adjust camera
            else if (eventArgs.KeyCode == r && viewportUpdate)
            {
                cameraHeight += movingSpeed;
            }
            else if (eventArgs.KeyCode == z && viewportUpdate)
            {
                cameraHeight -= movingSpeed;
            }
            else if (eventArgs.KeyCode == f && viewportUpdate)
            {
                cameraDistance += movingSpeed;
            }
            else if (eventArgs.KeyCode == c && viewportUpdate)
            {
                cameraDistance -= movingSpeed;
            }

            // Instrument toggle
            else if (eventArgs.KeyCode == Keys.L)
            {
                instrumentToggle = !instrumentToggle;
                instrument.PlanarPointEmitter.Active = instrumentToggle;
            }

            // Instrument Schedule
            else if (eventArgs.KeyCode == Keys.I)
            {
                emitting = !emitting;
                this.Schedule(200, emitting);
            }

            // Jumpping

            else
            {
                return;
            }

            ExpireSolution(true);
        }

        public void Schedule(int interval, bool emitting)
        {
            timer.Interval = interval;

            if (!emitting)
            {
                timer.Stop();
                timer.Elapsed -= OnTimerElapsed;
                return;
            }

            timer.Start();
            timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(Object sender, ElapsedEventArgs e)
        {
            this.instrument.PlanarPointEmitter.Trigger(instrument.PlanarPointEmitter.EmittingBoundary.Plane); ;
            Grasshopper.Instances.ActiveCanvas.Invoke(new Action(() => { ExpireSolution(true); }));
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
            get { return new Guid("FFB4C8F6-8B91-49CD-ABE4-85727F325C2F"); }
        }
    }
}