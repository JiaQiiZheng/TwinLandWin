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
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_GeometricGoo> projected = new GH_Structure<IGH_GeometricGoo>();

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

                // count the number of polygon shapes
                int polygonCount = 0;
                for (int j = 0; j < branch.Count; j++)
                {
                    var polygon = branch[j];
                    if (polygon != null && polygon.GetType() == GH_TypeLib.t_gh_brep)
                    {
                        polygonCount++;
                    }
                }

                Point3d[] centroids = new Point3d[polygonCount];
                Point3d[] pts_projected = new Point3d[0];


                // find all centroids of polygons
                for (int j = 0; j < polygonCount; j++)
                {
                    var polygon = branch[j];
                    if (polygon != null && polygon.GetType() == GH_TypeLib.t_gh_brep)
                    {
                        GH_Brep brep = polygon as GH_Brep;
                        AreaMassProperties amp = AreaMassProperties.Compute(brep.Value);
                        if (amp != null)
                        {
                            centroids[j] = amp.Centroid;
                        }
                    }
                }

                // project the mass centroid of polygons to target geometry
                if (geometry.TypeName == "Brep")
                {
                    pts_projected = Intersection.ProjectPointsToBreps(breps, centroids, direction, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                }
                else if (geometry.TypeName == "Mesh")
                {
                    pts_projected = Intersection.ProjectPointsToMeshes(meshes, centroids, direction, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                }

                // valify the projection count
                int originalCount = centroids.Length;
                if (polygonCount != originalCount)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid target geometry cause projected polygon count mismatch the original polygon count");
                    return;
                }


                // get vector from mass centroid of polygons to projected pts, then move polygon base on the its moving vector
                Vector3d[] vts = new Vector3d[originalCount];
                List<IGH_GeometricGoo> projectedBrep = new List<IGH_GeometricGoo>();

                for (int j = 0; j < originalCount; j++)
                {
                    Point3d mc = centroids[j];
                    Point3d pp = pts_projected[j];

                    Vector3d vt = new Vector3d(pp.X - mc.X, pp.Y - mc.Y, pp.Z - mc.Z);
                    vts[j] = vt;

                    var polygon = branch[j];
                    if (polygon.GetType() == GH_TypeLib.t_gh_brep)
                    {
                        GH_Brep originBrep_copy = ((GH_Brep)polygon).DuplicateBrep();

                        Transform motion = Transform.Translation(vt);
                        originBrep_copy.Transform(motion);
                        projectedBrep.Add(originBrep_copy);
                    }
                }

                // append projected polygon into the same path in output tree
                for (int j = 0; j < projectedBrep.Count; j++)
                {
                    projected.Append(projectedBrep[j], path);
                }
            }

            DA.SetDataTree(0, projected);
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