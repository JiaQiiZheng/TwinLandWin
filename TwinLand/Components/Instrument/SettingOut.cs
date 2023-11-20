using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Rhino;
using Rhino.Geometry;
using MIConvexHull;
using Grasshopper.Kernel.Geometry.Delaunay;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using System.Diagnostics;

namespace TwinLand.Components.Instrument
{
    public class SettingOut : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Setting Out class.
        /// </summary>
        public SettingOut()
          : base("Setting Out", "setting out",
              "Generate setting out curves on topography", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundary", "boundary", "", GH_ParamAccess.list);
            pManager.AddMeshParameter("Topography Mesh", "topography mesh", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Apply Dash Pattern", "apply dash pattern", "", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Row Distance", "row distance", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dash Length", "dash length", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Gap Length", "gap length", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Density", "density", "", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Seed", "seed", "", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Join", "join", "", GH_ParamAccess.item, true);

            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Vertices", "vertices", "", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Curve Setting Out", "curve setting out", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> boundaries = new List<Curve>();
            Mesh topo = null;
            double rowDistance = 0.0;
            bool applyDash = true;
            double dashLen = 0.0;
            double gapLen = 0.0;
            double density = 1.0;
            int seed = 1;
            bool join = true;

            DA.GetDataList("Boundary", boundaries);
            DA.GetData("Topography Mesh", ref topo);
            if (!DA.GetData("Row Distance", ref rowDistance)) return;
            DA.GetData("Dash Length", ref dashLen);
            DA.GetData("Gap Length", ref gapLen);
            DA.GetData("Density", ref density);
            DA.GetData("Seed", ref seed);
            DA.GetData("Join", ref join);


            List<Mesh> delMeshes = new List<Mesh>();
            GH_Structure<GH_Point> filteredPts = new GH_Structure<GH_Point>();
            GH_Structure<GH_Curve> contourTree = new GH_Structure<GH_Curve>();

            for (int i = 0; i < boundaries.Count; i++)
            {
                Curve crv = boundaries[i];

                if (crv.IsClosed)
                {
                    Point3d min = crv.GetBoundingBox(true).Corner(true, true, true);
                    Point3d max = crv.GetBoundingBox(true).Corner(false, false, true);

                    Rectangle2 rec = new Rectangle2(new Node2(min.X, min.Y), new Node2(max.X, max.Y));

                    GH_Path path = new GH_Path(i);
                    List<Curve> groupSegs = new List<Curve>();

                    // Filter vertices of input topo mesh
                    List<Point3d> pts = new List<Point3d>();

                    for (int j = 0; j < topo.Vertices.Count; j++)
                    {
                        if (rec.Includes(topo.Vertices[j].X, topo.Vertices[j].Y) == Containment.inside)
                        {
                            pts.Add(topo.Vertices[j]);
                        }
                    }

                    // Delaunay mesh generated from vertices
                    var nodes = new Node2List();
                    foreach (Point3d p in pts)
                    {
                        nodes.Append(new Node2(p.X, p.Y));
                    }

                    List<Face> faces = new List<Face>();
                    faces = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Faces(nodes, 0);

                    Mesh delMesh = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Mesh(nodes, 0, ref faces);
                    for (int j = 0; j < pts.Count; j++)
                    {
                        delMesh.Vertices.SetVertex(j, pts[j]);
                        if (crv.Contains(pts[j], Rhino.Geometry.Plane.WorldXY, tl) == PointContainment.Inside)
                        {
                            filteredPts.Append(new GH_Point(pts[j]), path);
                        }
                    }

                    delMeshes.Add(delMesh);

                    // Generate contour line based on distance
                    BoundingBox bbox_delMesh = delMesh.GetBoundingBox(true);
                    Point3d lowest = bbox_delMesh.Corner(true, true, false);
                    Point3d highest = bbox_delMesh.Corner(true, true, true);
                    Curve[] contours = Mesh.CreateContourCurves(delMesh, lowest, highest, rowDistance, tl);

                    for (int c = 0; c < contours.Length; c++)
                    {
                        Curve contour = contours[c];

                        // Apply dash pattern
                        Curve[] segs = null;

                        if (applyDash && dashLen != 0 && gapLen != 0)
                        {
                            // Reparameteraize contours
                            contour.Domain = new Interval(0, 1);

                            double len = contour.GetLength();
                            double segTotalLen = dashLen + gapLen;
                            int round = (int)Math.Floor(len / segTotalLen);

                            double curParam = 0;
                            double dashStep = dashLen / len;
                            double gapStep = gapLen / len;
                            double[] splitParams = new double[round * 2];

                            for (int j = 0; j < round; j++)
                            {
                                curParam += dashStep;
                                splitParams[j * 2] = curParam;
                                curParam += gapStep;
                                splitParams[j * 2 + 1] = curParam;
                            }

                            segs = contour.Split(splitParams);

                            Random rd_01 = new Random(seed + c);
                            int seed_02 = rd_01.Next();
                            for (int j = 0; j < segs.Length; j += 1)
                            {
                                // Apply density
                                Random rd_02 = new Random(seed_02 + j);

                                // Check whether the curve is inside curve
                                Curve seg = segs[j];
                                seg.Domain = new Interval(0, 1);
                                bool isInside = crv.Contains(seg.PointAt(0.5), Rhino.Geometry.Plane.WorldXY, tl) == PointContainment.Inside;

                                if (rd_02.NextDouble() < density && isInside)
                                {
                                    groupSegs.Add(seg);
                                }
                            }
                        }
                    }

                    // Join curves in this group
                    if (join)
                    {
                        Curve[] joinedCrvs = Curve.JoinCurves(groupSegs.ToArray(), tl);
                        foreach (Curve joinedSeg in joinedCrvs)
                        {
                            contourTree.Append(new GH_Curve(joinedSeg), path);
                        }
                    }
                    else
                    {
                        foreach (Curve unJoinedSeg in groupSegs)
                        {
                            contourTree.Append(new GH_Curve(unJoinedSeg), path);
                        }
                    }
                }
            }

            DA.SetDataTree(0, filteredPts);
            DA.SetDataTree(1, contourTree);
        }

        double tl = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

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
            get { return new Guid("B9E68874-F945-43A6-9CE7-A5D35C5D01E8"); }
        }
    }
}