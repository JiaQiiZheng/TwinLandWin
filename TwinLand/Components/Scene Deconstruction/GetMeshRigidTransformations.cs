using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;
using System.Xml;
using System.Diagnostics;
using Grasshopper.Kernel.Types;
using Rhino.Commands;
using System.Windows.Forms.VisualStyles;

namespace TwinLand.Components.Scene_Deconstruction
{
    public class GetMeshRigidTransformations : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the GetRigidTransformations class.
        /// </summary>
        public GetMeshRigidTransformations()
          : base("Get Mesh Rigid Transformations", "get mesh rigid transformations",
              "Description", "Deconstruct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FleX", "FleX", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Output Transformed Mesh", "output transformed mesh", "", GH_ParamAccess.item, true);
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.list);

            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Translations", "translations", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("Rotation Axis", "rotation axis", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rotation Angles", "rotation angles", "", GH_ParamAccess.list);
            pManager.AddTransformParameter("Transformations", "transformations", "", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Flex flex = null;
            bool outputMesh = true;

            if (!DA.GetData("FleX", ref flex)) { return; }
            DA.GetData("Output Transformed Mesh", ref outputMesh);
            List<Mesh> geos = new List<Mesh>();
            DA.GetDataList("Mesh", geos);

            List<Vector3d> translations = new List<Vector3d>();
            List<Vector3d> rotateAxis = new List<Vector3d>();
            List<double> rotateAngles = new List<double>();
            List<Transform> transformations = new List<Transform>();

            if (flex != null)
            {
                List<float> rot = flex.Scene.GetRigidRotations();
                List<float> trans = flex.Scene.GetRigidTranslations();
                List<float> massCenters = flex.Scene.GetShapeMassCenters();

                for (int i = 0; i < massCenters.Count/3; i++)
                {
                    double center_X = massCenters[i * 3];
                    double center_Y = massCenters[i * 3 + 1];
                    double center_Z = massCenters[i * 3 + 2];

                    translations.Add(new Vector3d(trans[i * 3] - center_X, trans[i * 3 + 1] - center_Y, trans[i * 3 + 2] - center_Z));

                    Vector3d rotationAxis = Vector3d.ZAxis;
                    double halfAngle = Math.Acos(rot[i * 4 + 3]);
                    double X = rot[i * 4] / Math.Sin(halfAngle);
                    double Y = rot[i * 4 + 1] / Math.Sin(halfAngle);
                    double Z = rot[i * 4 + 2] / Math.Sin(halfAngle);
                    if (HasValidValue(X) && HasValidValue(Y) && HasValidValue(Z))
                    {
                        rotationAxis = new Vector3d(X, Y, Z);
                    }

                    double angle = 2 * halfAngle;

                    Transform translation = Transform.Translation(translations[i]);
                    Transform rotation = Transform.Rotation(angle, rotationAxis, new Point3d(center_X, center_Y, center_Z));

                    Transform t = translation * rotation;

                    rotateAxis.Add(rotationAxis);
                    rotateAngles.Add(angle);
                    transformations.Add(t);

                    // If output set to true, output transformed geometry
                    if (outputMesh && geos.Count > i)
                    {
                        geos[i].Transform(t);
                    }
                }
            }


            DA.SetDataList("Translations", translations);
            DA.SetDataList("Rotation Axis", rotateAxis);
            DA.SetDataList("Rotation Angles", rotateAngles);
            DA.SetDataList("Transformations", transformations);

            DA.SetDataList("Mesh", geos);
        }

        /// <summary>
        /// Dynamic variables
        /// </summary>

        public static bool HasValidValue(double value)
        {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
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
            get { return new Guid("26F48BD1-CA2F-4DE7-A42A-4385F3093C88"); }
        }
    }
}