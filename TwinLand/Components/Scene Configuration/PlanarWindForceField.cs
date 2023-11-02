using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Eto.Forms;
using FlexCLI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Types;
using OSGeo.OGR;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace TwinLand.Components.Scene_Configuration
{
    public class PlanarWindForceField : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the WindForceField class.
        /// </summary>
        public PlanarWindForceField()
          : base("Planar Wind Force Field", "planar wind force field",
              "", "Configuration")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "plane", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Wind Tunnel Width", "wind tunnel width", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Wind Tunnel Height", "wind tunnel height", "", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Wind Tunnel Radius", "wind tunnel radius", "Only apply radius when wind tunnel shape set to round", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Wind Centroid Density", "wind centroid density", "", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("Force Field Radius", "force field radius", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Wind Force Strength", "wind force strength", "This parameter is meter/s", GH_ParamAccess.item);
            pManager.AddMeshParameter("Topography Mesh", "topography mesh", "", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Obstacles", "obstacles", "", GH_ParamAccess.list);
            //pManager.AddBooleanParameter("Wind Tunnel Round Shape On", "wind tunnel round shape on", "", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Mode", "mode", "0: Constant Force, 1: Impulse, 2: Velocity Change", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Linear Descent Off", "linear descent off ", "", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Force Field Display", "force field display", "", GH_ParamAccess.item, true);

            List<string> optionalParams = new List<string> { "Topography Mesh", "Obstacles", "Wind Tunnel Radius" };

            // Set specific params as Optional
            for (int i = 0; i < pManager.ParamCount; i++)
            {
                if (optionalParams.Contains(pManager[i].Name))
                {
                    pManager[i].Optional = true;
                    pManager[i].DataMapping = GH_DataMapping.Flatten;
                }
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Force Field", "force field", "", GH_ParamAccess.list);
            pManager.AddRectangleParameter("Wind Planar Boundary", "wind planar boundary", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Wind Centroids", "wind controids", "", GH_ParamAccess.list);
            pManager.AddCircleParameter("Force Field Boundary", "force field boundary", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rhino.Geometry.Plane plane = Rhino.Geometry.Plane.Unset;
            double width = 1.0;
            double height = 1.0;
            double radius = 1.0;
            double ff_radius = 10.0;
            List<float> ff_radius_intervened = new List<float>();
            int density = 3;

            double wfs = 1.0;

            List<Mesh> topo_list = new List<Mesh>();
            Point3d[] pt_hits_topo = null;
            Point3d[] pt_hits_obstacle_brep = null;
            Point3d[] pt_hits_obstacle_mesh = null;
            Point3d[] pt_lowest = null;
            List<IGH_GeometricGoo> obstacles = new List<IGH_GeometricGoo>();

            //bool roundTunnel = false;
            int mode = 0;
            bool linearOff = true;
            bool displayOn = true;

            // Output data containers
            Rectangle3d windPlanarBoundary = new Rectangle3d();
            List<Circle> ff_circles = new List<Circle>();

            // Collecting data
            if (!DA.GetData("Plane", ref plane)) return;

            DA.GetData("Wind Tunnel Width", ref width);
            DA.GetData("Wind Tunnel Height", ref height);
            //DA.GetData("Wind Tunnel Radius", ref radius);
            DA.GetData("Force Field Radius", ref ff_radius);
            DA.GetData("Wind Centroid Density", ref density);

            DA.GetData("Wind Force Strength", ref wfs);

            DA.GetDataList("Topography Mesh", topo_list);
            Mesh[] topos = topo_list.ToArray();

            DA.GetDataList("Obstacles", obstacles);
            Mesh[] obstacle_meshes = null;
            Brep[] obstacle_breps = null;
            List<Mesh> obstacle_mesh_list = new List<Mesh>();
            List<Brep> obstacle_brep_list = new List<Brep>();
            foreach (var obs in obstacles)
            {
                if (obs == null) continue;
                if (obs.GetType().Name == "GH_Brep")
                {
                    GH_Brep gh_brep = obs as GH_Brep;
                    obstacle_brep_list.Add(gh_brep.Value);
                }
                else if (obs.GetType().Name == "GH_Box")
                {
                    GH_Box box = obs as GH_Box;
                    GH_Brep convertToBrep = new GH_Brep(Brep.CreateFromBox(box.Value));
                    obstacle_brep_list.Add(convertToBrep.Value);
                }
                else if (obs.GetType().Name == "GH_Mesh")
                {
                    GH_Mesh gh_mesh = obs as GH_Mesh;
                    obstacle_mesh_list.Add(gh_mesh.Value);
                }
            }
            obstacle_breps = obstacle_brep_list.ToArray();
            obstacle_meshes = obstacle_mesh_list.ToArray();

            //DA.GetData("Wind Tunnel Round Shape On", ref roundTunnel);
            DA.GetData("Mode", ref mode);
            DA.GetData("Linear Descent Off", ref linearOff);

            // display
            DA.GetData("Force Field Display", ref displayOn);

            // Generate wind centroid grids
            Interval w_domain = new Interval(-width / 2, width / 2);
            Interval h_domain = new Interval(-height / 2, height / 2);
            windPlanarBoundary = new Rectangle3d(plane, w_domain, h_domain);

            List<Point3d> pts = new List<Point3d>();
            int w_count = density != 0 ? density : 1;
            int h_count = (int)(height / width * w_count);

            if (h_count == 0)
            {
                h_count = 1;
            }

            for (int i = 0; i <= w_count; i++)
            {
                double param_w = i / (double)w_count;
                for (int j = 0; j <= h_count; j++)
                {
                    // Initial force field radius for shinking calculation
                    DA.GetData("Force Field Radius", ref ff_radius);

                    double param_h = j / (double)h_count;
                    Point3d pt = new Point3d(windPlanarBoundary.PointAt(param_w, param_h));
                    pts.Add(pt);

                    Point3d[] pt_origin = new Point3d[] { pt };

                    // Project wind centroids onto DEM along plane normal(wind direction)
                    pt_hits_topo = Intersection.ProjectPointsToMeshes(topos, pt_origin, plane.Normal, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                    // Project wind centroids onto all obstacle geometry along plane normal(wind direction)
                    pt_hits_obstacle_brep = Intersection.ProjectPointsToBreps(obstacle_breps, pt_origin, plane.Normal, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    pt_hits_obstacle_mesh = Intersection.ProjectPointsToMeshes(obstacle_meshes, pt_origin, plane.Normal, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                    // Initial collecton of pt_hits
                    int topoCount = pt_hits_topo != null ? pt_hits_topo.Length : 0;
                    int brepCount = pt_hits_obstacle_brep != null ? pt_hits_obstacle_brep.Length : 0;
                    int meshCount = pt_hits_obstacle_mesh != null ? pt_hits_obstacle_mesh.Length : 0;

                    Point3d[] pt_hits = new Point3d[topoCount + brepCount + meshCount];
                    // merge all pt_hits together
                    if (pt_hits_topo != null)
                    {
                        pt_hits_topo.CopyTo(pt_hits, 0);
                    }
                    if (pt_hits_obstacle_brep != null)
                    {
                        pt_hits_obstacle_brep.CopyTo(pt_hits, pt_hits_topo.Length);
                    }
                    if (pt_hits_obstacle_mesh != null)
                    {
                        pt_hits_obstacle_mesh.CopyTo(pt_hits, pt_hits_obstacle_brep.Length);
                    }


                    // Check if pt is under Mesh
                    pt_lowest = Intersection.ProjectPointsToMeshes(topos, pt_origin, new Vector3d(0, 0, -1), RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    double minZ = double.MinValue;
                    if (pt_lowest == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Wind tunnel is not above topography mesh or topography mesh not being set yet.");
                        return;
                    }
                    foreach (var low_hit in pt_lowest)
                    {
                        minZ = Math.Max(minZ, low_hit.Z);
                    }

                    // Check if pt is inside obstacles
                    bool insideObstacle = false;
                    foreach (Brep b in obstacle_breps)
                    {
                        if (b.IsPointInside(pt, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, true))
                        {
                            insideObstacle = true;
                        }
                    }
                    foreach (Mesh m in obstacle_meshes)
                    {
                        if (m.IsPointInside(pt, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, true))
                        {
                            insideObstacle = true;
                        }
                    }

                    // set invalid wind controid's radius to 0
                    if (pt.Z <= minZ || insideObstacle)
                    {
                        ff_radius = 0;
                    }
                    // set valid wind centroid's radius to minimum run distance
                    else
                    {
                        if (pt_hits != null && pt_hits.Length > 0)
                        {
                            foreach (var hit in pt_hits)
                            {
                                ff_radius = Math.Min(ff_radius, pt.DistanceTo(hit));
                            }
                        }
                    }

                    // Add topo or obstacles intevened force field radius into list
                    ff_radius_intervened.Add((float)ff_radius);
                    ff_circles.Add(new Circle(pt, ff_radius));
                }
            }

            List<FlexForceField> ffs = new List<FlexForceField>();
            for (int i = 0; i < pts.Count; i++)
            {
                float[] positon = { (float)pts[i].X, (float)pts[i].Y, (float)pts[i].Z };
                ffs.Add(new FlexForceField(positon, ff_radius_intervened[i], (float)wfs, linearOff, mode));
            }

            DA.SetDataList("Force Field", ffs);
            DA.SetData("Wind Planar Boundary", windPlanarBoundary);
            DA.SetDataList("Wind Centroids", pts);
            if (displayOn)
            {
                DA.SetDataList("Force Field Boundary", ff_circles);
            }
        }


        /// <summary>
        /// Dynamic variables
        /// </summary>
        Random rd = new Random();

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
            get { return new Guid("B6AFEB77-4A18-4BA1-8DCD-4386D8B16E5F"); }
        }
    }
}