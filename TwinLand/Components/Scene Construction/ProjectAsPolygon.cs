using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GH_IO.Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using OSGeo.OGR;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Runtime.InteropWrappers;

namespace TwinLand.Components.Scene_Construction
{
    public class ProjectAsPolygon : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the ProjectAsPolygon class.
        /// </summary>
        public ProjectAsPolygon()
          : base("Project as Polygon", "project as polygon",
              "Project OSM data to geometry as polygon", "Construct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("OSM Features", "osm features", "The features data from OpenStreetMap", GH_ParamAccess.tree);
            pManager.AddGeometryParameter("Geometry", "geometry", "The target geometry of the projection", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Polygon", "polygon", "Projected polygon", GH_ParamAccess.tree);
            pManager.AddPointParameter("Project Points", "project pts", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_GeometricGoo> projected = new GH_Structure<IGH_GeometricGoo>();
            GH_Structure<GH_Point> projectedPt = new GH_Structure<GH_Point>();

            GH_Structure<IGH_GeometricGoo> data = new GH_Structure<IGH_GeometricGoo>();
            IGH_GeometricGoo geometry = null;
            Vector3d direction = Vector3d.ZAxis;

            if (!DA.GetDataTree("OSM Features", out data)) { return; }
            if (!DA.GetData("Geometry", ref geometry)) { return; }

            Brep[] breps = new Brep[1];
            Mesh[] meshes = new Mesh[1];

            if (geometry.TypeName == "Brep")
            {
                GH_Brep gh_brep = new GH_Brep(geometry as GH_Brep);
                Brep brep_target = gh_brep.Value;
                breps[0] = brep_target;
            }
            else if (geometry.TypeName == "Mesh")
            {
                GH_Mesh gh_mesh = new GH_Mesh(geometry as GH_Mesh);
                Mesh mesh_target = gh_mesh.Value;
                meshes[0] = mesh_target;
            }

            List<Point3d> pts = new List<Point3d>();

            for (int i = 0; i < data.PathCount; i++)
            {
                GH_Path path = new GH_Path(data.get_Path(i));
                var branch = data.get_Branch(path);
                int dataCount = branch.Count;

                // count the number of polygon shapes
                List<Point3d> centroids_list = new List<Point3d>();

                for (int j = 0; j < dataCount; j++)
                {
                    var polygon = branch[j];

                    // find all centroids of polygons
                    if (polygon != null && polygon.GetType() == GH_TypeLib.t_gh_brep)
                    {
                        GH_Brep brep = polygon as GH_Brep;
                        AreaMassProperties amp = AreaMassProperties.Compute(brep.Value);
                        if (amp != null)
                        {
                            // start to project
                            Point3d[] centroids = new Point3d[1];
                            Point3d[] pts_projected = new Point3d[1];

                            centroids[0] = amp.Centroid;

                            pts_projected = Intersection.ProjectPointsToMeshes(meshes, centroids, direction, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                            Point3d mc = centroids[0];
                            Point3d pp = pts_projected.Length>0? pts_projected[0] : Point3d.Unset;

                            if (mc != null && pp != Point3d.Unset)
                            {
                                Vector3d vt = new Vector3d(pp.X - mc.X, pp.Y - mc.Y, pp.Z - mc.Z);
                                Transform motion = Transform.Translation(vt);
                                GH_Brep gh_polygon = polygon as GH_Brep;
                                gh_polygon.Transform(motion);

                                projected.Append(gh_polygon, path);
                                projectedPt.Append(new GH_Point(pp), path);
                            }
                        }
                    }
                }
            }

            DA.SetDataTree(0, projected);
            DA.SetDataTree(1, projectedPt);
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
            get { return new Guid("22287717-8238-4677-B671-E35FB5553B3B"); }
        }
    }
}