using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;

namespace TwinLand.Components.Instrument.Fence
{
    public class Fence : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the SandFence class.
        /// </summary>
        public Fence()
          : base("Fence", "fence",
              "", "Instrument")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Setting Out", "setting out", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Width", "width", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Length", "length", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Height", "height", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Depth", "depth", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Gap", "gap", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Angle", "angle", "", GH_ParamAccess.tree, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Fence", "fence", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Curve> settingOut_tree = new GH_Structure<GH_Curve>();
            GH_Structure<GH_Number> width_tree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> length_tree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> height_tree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> depth_tree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> gap_tree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> angle_tree = new GH_Structure<GH_Number>();

            if (!DA.GetDataTree("Setting Out", out settingOut_tree)) return;
            if (!DA.GetDataTree("Width", out width_tree)) return;
            if (!DA.GetDataTree("Length", out length_tree)) return;
            if (!DA.GetDataTree("Height", out height_tree)) return;
            if (!DA.GetDataTree("Depth", out depth_tree)) return;
            if (!DA.GetDataTree("Gap", out gap_tree)) return;
            DA.GetDataTree("Angle", out angle_tree);

            GH_Structure<GH_Box> fences = new GH_Structure<GH_Box>();

            for (int i = 0; i < settingOut_tree.Branches.Count; i++)
            {
                var branch = settingOut_tree.get_Branch(i);
                GH_Path path = settingOut_tree.get_Path(i);
                for (int j = 0; j < branch.Count; j++)
                {
                    Curve crv = settingOut_tree.get_DataItem(path, j).Value;
                    double width = width_tree.PathExists(path) && width_tree.get_Branch(path).Count > j ? width_tree.get_DataItem(path, j).Value : width_tree.get_DataItem(0).Value;
                    double length = length_tree.PathExists(path) && length_tree.get_Branch(path).Count > j ? length_tree.get_DataItem(path, j).Value : length_tree.get_DataItem(0).Value;
                    double height = height_tree.PathExists(path) && height_tree.get_Branch(path).Count > j ? height_tree.get_DataItem(path, j).Value : height_tree.get_DataItem(0).Value;
                    double depth = depth_tree.PathExists(path) && depth_tree.get_Branch(path).Count > j ? depth_tree.get_DataItem(path, j).Value : depth_tree.get_DataItem(0).Value;
                    double gap = gap_tree.PathExists(path) && gap_tree.get_Branch(path).Count > j ? gap_tree.get_DataItem(path, j).Value : gap_tree.get_DataItem(0).Value;
                    double angle = angle_tree.PathExists(path) && angle_tree.get_Branch(path).Count > j ? angle_tree.get_DataItem(path, j).Value : angle_tree.get_DataItem(0).Value;

                    // Divide setting out based on width and gap
                    crv.Domain = new Interval(0, 1);
                    double curParam = 0.0;

                    while (curParam <= 1)
                    {
                        Point3d fenceCenter = crv.PointAt(curParam);

                        // Project the tangent into 2d vector
                        Vector3d tangent = new Vector3d(crv.TangentAt(curParam).X, crv.TangentAt(curParam).Y, 0);
                        tangent.Unitize();
                        tangent.Rotate(RhinoMath.ToRadians(angle), Vector3d.ZAxis);
                        Plane fencePlane = new Plane(fenceCenter, tangent, Vector3d.ZAxis);
                        Box fenceBox = new Box(fencePlane, new Interval(-width / 2, width / 2), new Interval(-depth, height), new Interval(-length / 2, length / 2));

                        fences.Append(new GH_Box(fenceBox), path);

                        double totalStep = (width + gap) / crv.GetLength();
                        curParam += totalStep;

                    }
                }
            }

            DA.SetDataTree(0, fences);
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
            get { return new Guid("227B4DC9-9996-445E-BED4-ACBF05B9A5FA"); }
        }
    }
}