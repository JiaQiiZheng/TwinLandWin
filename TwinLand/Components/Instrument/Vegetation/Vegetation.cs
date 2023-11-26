using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Eto;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MIConvexHull;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.UI.Forms.ViewModels;

namespace TwinLand.Components.Instrument.Vegetation
{
    public class Vegetation : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the Vegetation class.
        /// </summary>
        public Vegetation()
          : base("Vegetation", "vegetation",
              "Place vegetation with custom shape", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Topography Mesh", "topography mesh", "", GH_ParamAccess.item);
            pManager.AddCurveParameter("Setting Out Curve", "setting out curve", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Thickness", "thickness", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "height", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Depth", "depth", "", GH_ParamAccess.item, 100.0);
            pManager.AddNumberParameter("Gap", "gap", "", GH_ParamAccess.item, 100.0);
            pManager.AddNumberParameter("Thickness Randomness", "thickness randomness", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Height Randomness", "height randomness", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Depth Randomness", "depth randomness", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Gap Randomness", "gap randomness", "", GH_ParamAccess.item, 0.0);
            pManager.AddIntegerParameter("Distribution Mode", "distribution mode", "", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Accuracy", "accuracy", "1-low, 10-high", GH_ParamAccess.item, 8);
            pManager.AddBooleanParameter("Attach", "attach", "", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Volumn Mesh", "volumn mesh", "", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Area", "area", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh topo = null;
            GH_Structure<GH_Curve> settingOutCrvs = new GH_Structure<GH_Curve>();
            double thickness = 100;
            double height = 100;
            double depth = 100;
            double gap = 100;
            double rd_thickness = 0.0;
            double rd_height = 0.0;
            double rd_depth = 0.0;
            double rd_gap = 0.0;
            int mode = 0;
            int accuracy = 8;
            bool attach = true;

            if (!DA.GetData("Topography Mesh", ref topo)) return;
            if (!DA.GetDataTree("Setting Out Curve", out settingOutCrvs)) return;
            DA.GetData("Thickness", ref thickness);
            DA.GetData("Height", ref height);
            DA.GetData("Depth", ref depth);
            DA.GetData("Gap", ref gap);
            DA.GetData("Thickness Randomness", ref rd_thickness);
            DA.GetData("Height Randomness", ref rd_height);
            DA.GetData("Depth Randomness", ref rd_depth);
            DA.GetData("Gap Randomness", ref rd_gap);
            DA.GetData("Distribution Mode", ref mode);
            DA.GetData("Accuracy", ref accuracy);
            DA.GetData("Attach", ref attach);

            // Adjust accuary to 1 to 10
            accuracy = TwinLand.Helper.Adjust(accuracy, 1, 10);

            GH_Structure<GH_Mesh> volumns = new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Curve> areas = new GH_Structure<GH_Curve>();

            for (int i = 0; i < settingOutCrvs.Branches.Count; i++)
            {
                GH_Path path = settingOutCrvs.get_Path(i);

                for (int j = 0; j < settingOutCrvs.get_Branch(path).Count; j++)
                {
                    // Apply randomness factor
                    Random rd = new Random(j);
                    double thickness_cur = thickness * (1 + rd.NextDouble() * rd_thickness);
                    double height_cur = height * (1 + rd.NextDouble() * rd_height);
                    double depth_cur = depth * (1 + rd.NextDouble() * rd_depth);
                    double gap_cur = gap * (1 + rd.NextDouble() * rd_gap);

                    Curve crv = settingOutCrvs.get_DataItem(path, j).Value;

                    // Point distribution
                    if (mode == 0)
                    {
                        double[] pt_params = crv.DivideByLength(thickness_cur + gap_cur, false);
                        if (pt_params == null) continue;
                        foreach (var p in pt_params)
                        {
                            Point3d center = crv.PointAt(p);
                            Curve profile = DrawPolygon(center, thickness_cur / 2, accuracy * 2 + 1);

                            // EXtrude process
                            Mesh merged = ExtrudeUp(profile, height_cur, depth_cur, accuracy);

                            if (attach)
                            {
                                profile = MeshMesh(topo, merged);

                                // Make sure all closed curve are clockwise
                                TwinLand.Helper.UnifyCLosedCurveOrientation(profile, false);

                                merged = ExtrudeUp(profile, height_cur, depth_cur, accuracy);
                            }

                            // Output
                            volumns.Append(new GH_Mesh(merged), path);
                            areas.Append(new GH_Curve(profile), path);
                        }
                    }

                    // Linear distribution
                    else if (mode == 1)
                    {
                        Curve[] joined = OffsetToBothSide(crv, thickness_cur);
                        if (joined == null || joined.Length != 1) continue;
                        Curve profile = joined[0];

                        // Make sure all closed curve are clockwise

                        // Extrude process
                        Mesh merged = ExtrudeUp(profile, height_cur, depth_cur, accuracy);

                        if (attach)
                        {
                            profile = MeshMesh(topo, merged);

                            // Make sure all closed curve are clockwise
                            TwinLand.Helper.UnifyCLosedCurveOrientation(profile, false);

                            merged = ExtrudeUp(profile, height_cur, depth_cur, accuracy);
                        }


                        // Output
                        volumns.Append(new GH_Mesh(merged), path);
                        areas.Append(new GH_Curve(profile), path);
                    }
                }
            }

            DA.SetDataTree(0, volumns);
            DA.SetDataTree(1, areas);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>
        double tl = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;


        /// <summary>
        /// Additional method
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="direction"></param>
        /// <param name="extrusionHeight"></param>
        /// <returns></returns>
        public static Mesh ExtrudePolylineOneSide(Polyline polyline, Vector3d direction, double extrusionHeight)
        {
            Mesh meshExtruded = new Mesh();

            direction.Unitize();
            foreach (Point3d pt in polyline)
            {
                meshExtruded.Vertices.Add(pt);
                meshExtruded.Vertices.Add(pt + direction * extrusionHeight);
            }

            for (int i = 0; i < polyline.Count - 1; i++)
            {
                meshExtruded.Faces.AddFace(i * 2, i * 2 + 1, (i + 1) * 2 + 1, (i + 1) * 2);
            }

            meshExtruded.Vertices.CullUnused();
            meshExtruded.Vertices.CombineIdentical(true, true);
            meshExtruded.RebuildNormals();

            return meshExtruded;
        }

        public Curve[] OffsetToBothSide(Curve crv, double thickness_cur)
        {
            Curve[] left = null;
            Curve[] right = null;

            // Offset to both sides
            left = crv.Offset(Plane.WorldXY, thickness_cur / 2, tl, CurveOffsetCornerStyle.Sharp);
            right = crv.Offset(Plane.WorldXY, -thickness_cur / 2, tl, CurveOffsetCornerStyle.Sharp);
            // Check if the offset result is correct or not
            if (left.Length != 1 || right.Length != 1) return null;

            Point3d start_left = left[0].PointAtStart;
            Point3d end_left = left[0].PointAtEnd;

            Point3d start_right = right[0].PointAtStart;
            Point3d end_right = right[0].PointAtEnd;

            Line left_cap = new Line(start_left, start_right);
            Line right_cap = new Line(end_left, end_right);

            Curve[] joined = Curve.JoinCurves(new Curve[4] { left[0], left_cap.ToNurbsCurve(), right[0], right_cap.ToNurbsCurve() }, tl);

            return joined;
        }

        public Mesh ExtrudeBothSide(Curve profile, double height_cur, double depth_cur, int accuracy)
        {
            Surface extrudeUp = Surface.CreateExtrusion(profile, new Vector3d(0, 0, height_cur));
            Surface extrudeDown = Surface.CreateExtrusion(profile, new Vector3d(0, 0, -depth_cur));

            MeshingParameters mp = new MeshingParameters(1.0, height_cur / accuracy);
            Mesh meshUp = Mesh.CreateFromSurface(extrudeUp, mp);
            Mesh meshDown = Mesh.CreateFromSurface(extrudeDown, mp);

            Mesh merged = new Mesh();
            merged.Append(new Mesh[] { meshUp, meshDown });

            return merged;
        }

        public Mesh ExtrudeUp(Curve profile, double height_cur, double depth_cur, int accuracy)
        {
            Curve moveProfile = profile.Duplicate() as Curve;
            moveProfile.Translate(new Vector3d(0, 0, -depth_cur));
            Surface sur = Surface.CreateExtrusion(moveProfile, new Vector3d(0, 0, depth_cur + height_cur));

            MeshingParameters mp = new MeshingParameters(1.0, height_cur / accuracy);
            Mesh extruded = new Mesh();
            extruded = Mesh.CreateFromSurface(sur, mp);

            return extruded;
        }

        public Curve MeshMesh(Mesh mesh_1, Mesh mesh_2)
        {
            Polyline[] intersections = null;
            intersections = Intersection.MeshMeshAccurate(mesh_1, mesh_2, tl);
            return intersections[0].ToNurbsCurve();
        }

        public Mesh Loft(Curve profile, double height_cur, double depth_cur, int accuracy)
        {
            profile.Translate(new Vector3d(0, 0, -depth_cur));

            //TODO.create from loft method
            Brep bp = null;

            MeshingParameters mp = new MeshingParameters(1.0, height_cur / accuracy);
            Mesh[] extruded = null;
            extruded = Mesh.CreateFromBrep(bp);

            return extruded[0];
        }

        public Curve DrawPolygon(Point3d center, double radius, int sides)
        {
            // Create a polygon
            Polyline polyline = new Polyline();
            for (int i = 0; i <= sides; i++)
            {
                double angle = 2 * Math.PI * i / sides;
                double x = radius * Math.Cos(angle) + center.X;
                double y = radius * Math.Sin(angle) + center.Y;
                polyline.Add(new Point3d(x, y, center.Z));
            }

            Curve polygon = polyline.ToNurbsCurve();

            return polygon;
        }

        public Mesh Triangulate(Mesh x)
        {
            int facecount = x.Faces.Count;
            for (int i = 0; i < facecount; i++)
            {
                var mf = x.Faces[i];
                if (mf.IsQuad)
                {
                    double dist1 = x.Vertices[mf.A].DistanceTo(x.Vertices[mf.C]);
                    double dist2 = x.Vertices[mf.B].DistanceTo(x.Vertices[mf.D]);
                    if (dist1 > dist2)
                    {
                        x.Faces.AddFace(mf.A, mf.B, mf.D);
                        x.Faces.AddFace(mf.B, mf.C, mf.D);
                    }
                    else
                    {
                        x.Faces.AddFace(mf.A, mf.B, mf.C);
                        x.Faces.AddFace(mf.A, mf.C, mf.D);
                    }
                }
            }

            var newfaces = new List<MeshFace>();
            foreach (var mf in x.Faces)
            {
                if (mf.IsTriangle) newfaces.Add(mf);
            }

            x.Faces.Clear();
            x.Faces.AddFaces(newfaces);
            return x;
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
            get { return new Guid("A972EF13-A362-49E0-A396-C36BF840E56F"); }
        }
    }
}