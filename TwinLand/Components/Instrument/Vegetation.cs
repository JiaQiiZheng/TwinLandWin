using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MIConvexHull;
using Rhino;
using Rhino.Geometry;
using Rhino.UI.Forms.ViewModels;

namespace TwinLand.Components.Instrument
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
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Setting Out Curve", "setting out curve", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Thickness", "thickness", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "height", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Depth", "depth", "", GH_ParamAccess.item, 100.0);
            pManager.AddNumberParameter("Thickness Randomness", "thickness randomness", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Height Randomness", "height randomness", "", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Depth Randomness", "depth randomness", "", GH_ParamAccess.item, 0.0);
            pManager.AddIntegerParameter("Alignment", "Alignment", "0-left, 1-center, 2-right", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Volumn Mesh", "volumn mesh", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Curve> settingOutCrvs = new GH_Structure<GH_Curve>();
            double thickness = 100;
            double height = 100;
            double depth = 100;
            double rd_thickness = 0.0;
            double rd_height = 0.0;
            double rd_depth = 0.0;
            int alignment = 1;

            if (!DA.GetDataTree("Setting Out Curve", out settingOutCrvs)) return;
            DA.GetData("Thickness", ref thickness);
            DA.GetData("Height", ref height);
            DA.GetData("Depth", ref depth);
            DA.GetData("Thickness Randomness", ref rd_thickness);
            DA.GetData("Height Randomness", ref rd_height);
            DA.GetData("Depth Randomness", ref rd_depth);
            DA.GetData("Alignment", ref alignment);

            

            double tl = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            GH_Structure<GH_Mesh> volumns = new GH_Structure<GH_Mesh>();

            for (int i = 0; i < settingOutCrvs.Branches.Count; i++)
            {
                GH_Path path = settingOutCrvs.get_Path(i);
                for (int j = 0; j < settingOutCrvs.get_Branch(path).Count; j++)
                {
                    // Apply randomness factor
                    Random rd = new Random(j);
                    double thickness_cur = thickness * (1 + (rd.NextDouble() * rd_thickness));
                    double height_cur = height * (1 + (rd.NextDouble() * rd_height));
                    double depth_cur = depth * (1 + (rd.NextDouble() * rd_depth));

                    Curve crv = settingOutCrvs.get_DataItem(path, j).Value;
                    Curve[] left = null;
                    Curve[] right = null;

                    if (alignment == 0)
                    {
                        left = crv.Offset(Plane.WorldXY, thickness_cur, tl, CurveOffsetCornerStyle.Sharp);
                        right[0] = crv;
                    }
                    else if (alignment == 1)
                    {
                        // Offset to both sides
                        left = crv.Offset(Plane.WorldXY, thickness_cur / 2, tl, CurveOffsetCornerStyle.Sharp);
                        right = crv.Offset(Plane.WorldXY, -thickness_cur / 2, tl, CurveOffsetCornerStyle.Sharp);
                    }
                    else
                    {
                        left[0] = crv;
                        right = crv.Offset(Plane.WorldXY, -thickness_cur, tl, CurveOffsetCornerStyle.Sharp);
                    }

                    if (left.Length != 1 || right.Length != 1) continue;

                    Point3d start_left = left[0].PointAtStart;
                    Point3d end_left = left[0].PointAtEnd;

                    Point3d start_right = right[0].PointAtStart;
                    Point3d end_right = right[0].PointAtEnd;

                    Line left_cap = new Line(start_left, start_right);
                    Line right_cap = new Line(end_left, end_right);

                    Curve[] joined = Curve.JoinCurves(new Curve[4] { left[0], left_cap.ToNurbsCurve(), right[0], right_cap.ToNurbsCurve() }, tl);
                    if (joined.Length != 1) continue;
                    Curve profile = joined[0];

                    // Extrude profile as enclosed mesh
                    Vector3d down = new Vector3d(0, 0, -depth);

                    profile.Translate(down);

                    Polyline profilePolyLine = profile.ToPolyline(0, 0, 0, double.MaxValue).ToPolyline();
                    Mesh extrudeUp = ExtrudePolylineOneSide(profilePolyLine, new Vector3d(0, 0, 1), height_cur+depth_cur);
                    extrudeUp = Triangulate(extrudeUp);

                    Polyline[] edges = extrudeUp.GetNakedEdges();

                    foreach (Polyline edge in edges)
                    {
                        extrudeUp.Append(Mesh.CreateFromClosedPolyline(edge));
                    }

                    volumns.Append(new GH_Mesh(extrudeUp), path);
                }
            }

            DA.SetDataTree(0, volumns);
        }

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

            for (int i = 0; i < (polyline.Count - 1); i++)
            {
                meshExtruded.Faces.AddFace(i * 2, i * 2 + 1, (i + 1) * 2 + 1, (i + 1) * 2);
            }

            meshExtruded.Vertices.CullUnused();
            meshExtruded.Vertices.CombineIdentical(true, true);
            meshExtruded.RebuildNormals();

            return meshExtruded;
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