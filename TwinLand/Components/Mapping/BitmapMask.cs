using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand.Components.Mapping
{
    public class BitmapMask : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the BitmapMask class.
        /// </summary>
        public BitmapMask()
          : base("Bitmap Mask", "bitmap mask",
              "n", "Mapping")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundary", "boundary", "", GH_ParamAccess.item);
            pManager.AddCurveParameter("Mask Area", "mask area", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Target Folder", "target folder", "", GH_ParamAccess.item);
            pManager.AddTextParameter("File Name", "file name", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Resolution", "resolution", "", GH_ParamAccess.item, 2048);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Bitmap Mask", "bitmap mask", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve boundary = null;
            List<Curve> maskAreas = new List<Curve>();
            string targetFolder = String.Empty;
            string fileName = String.Empty;
            int resolution = 1024;

            if (!DA.GetData("Boundary", ref boundary)) return;
            if (!DA.GetDataList("Mask Area", maskAreas)) return;
            if (!DA.GetData("Target Folder", ref targetFolder)) return;
            if (!DA.GetData("File Name", ref fileName)) return;
            DA.GetData("Resolution", ref resolution);

            // Draw a bitmap
            Bitmap bmp = DrawMask(boundary, maskAreas, resolution);

            // Save the bitmap
            string filePath = System.IO.Path.Combine(targetFolder, fileName);
            SaveBitmapToFile(bmp, filePath);

            DA.SetData("Bitmap Mask", filePath);
        }

        /// <summary>
        /// Additional methods
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="maskAreas"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        private Bitmap DrawMask(Curve boundary, List<Curve> maskAreas, int resolution)
        {
            Bitmap bmp = new Bitmap(resolution, resolution);
            Point3d min = boundary.GetBoundingBox(true).Min;
            Point3d max = boundary.GetBoundingBox(true).Max;

            double x = resolution / (max.X - min.X);
            double y = resolution / (max.Y - min.Y);

            Point3d center = (min + max) / 2;
            Transform scale = Transform.Scale(new Plane(center, Vector3d.ZAxis), x, y, 1);

            boundary.Transform(scale);

            // Draw a black mask
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.FillRectangle(Brushes.White, 0, 0, resolution, resolution);

                foreach (Curve area in maskAreas)
                {
                    area.Transform(scale);

                    BoundingBox bb = area.GetBoundingBox(true);

                    float start_x = (float)bb.Min.X + resolution / 2;
                    float start_y = -(float)bb.Max.Y + resolution / 2;
                    double width = bb.Max.X - bb.Min.X;
                    double height = bb.Max.Y - bb.Min.Y;

                    graphics.FillRectangle(Brushes.Black, start_x, start_y, (float)width, (float)height);
                }
            }

            return bmp;
        }

        private void SaveBitmapToFile(Bitmap bitmap, string filePath)
        {
            // Save the bitmap to the specified file path
            bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
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
            get { return new Guid("3B04B56A-8CCA-4C58-B526-59CFD8FC2D2E"); }
        }
    }
}