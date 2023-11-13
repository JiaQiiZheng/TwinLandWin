using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using FlexCLI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Serialization;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Deconstruction
{
    public class GetRigidTransformations : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the GetBrepRigidTransformations class.
        /// </summary>
        public GetRigidTransformations()
          : base("Get Rigid Transformations", "get rigid transformations",
              "", "Deconstruction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FleX", "FleX", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Apply Transformation", "apply transformation", "", GH_ParamAccess.item, true);
            pManager.AddGeometryParameter("Rigid Body Geometry", "rigid body geometry", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Group Rigid Body Count", "group rigid body count", "", GH_ParamAccess.list);

            pManager[2].Optional = true;
            pManager[2].DataMapping = GH_DataMapping.Flatten;
            pManager[3].Optional = true;
            pManager[3].DataMapping = GH_DataMapping.Flatten;
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
            pManager.AddGeometryParameter("Rigid Body Geometry", "rigid body geometry", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Flex flex = null;
            bool applyTransformation = true;

            if (!DA.GetData("FleX", ref flex)) { return; }
            DA.GetData("Apply Transformation", ref applyTransformation);

            List<GeometryBase> geos = new List<GeometryBase>();
            DA.GetDataList("Rigid Body Geometry", geos);
            List<GeometryBase> geo_list = geos;

            List<int> groupCount = new List<int>();
            DA.GetDataList("Group Rigid Body Count", groupCount);

            //List<RigidBody> rbs = new List<RigidBody>();
            //DA.GetDataList("Rigid Body", rbs);


            List<Vector3d> translations = new List<Vector3d>();
            List<Vector3d> rotateAxis = new List<Vector3d>();
            List<double> rotateAngles = new List<double>();
            List<Transform> transformations = new List<Transform>();
            //GH_Structure<GH_Vector> translations = new GH_Structure<GH_Vector>();
            //GH_Structure<GH_Vector> rotateAxis = new GH_Structure<GH_Vector>();
            //GH_Structure<GH_Number> rotateAngles = new GH_Structure<GH_Number>();
            //GH_Structure<GH_Transform> transformations = new GH_Structure<GH_Transform>();

            if (flex != null)
            {
                List<float> rot = flex.Scene.GetRigidRotations();
                List<float> trans = flex.Scene.GetRigidTranslations();
                List<float> massCenters = flex.Scene.GetShapeMassCenters();

                int round = massCenters.Count / 3;

                //int counter = 0;
                //for (int i = 0; i < geos.Branches.Count; i++)
                //{
                //    for (int j = 0; j < geos.Branches[i].Count; j++)
                //    {
                //        counter++;
                //        GH_Path path = new GH_Path(i);

                //        double center_X = massCenters[counter * 3 - 3];
                //        double center_Y = massCenters[counter * 3 - 2];
                //        double center_Z = massCenters[counter * 3 - 1];

                //        //translations.Add(new Vector3d(trans[counter * 3] - center_X, trans[counter * 3 + 1] - center_Y, trans[counter * 3 + 2] - center_Z));
                //        translations.Append(new GH_Vector(new Vector3d(trans[counter * 3 - 3] - center_X, trans[counter * 3 - 2] - center_Y, trans[counter * 3 - 1] - center_Z)), path);

                //        Vector3d rotationAxis = Vector3d.ZAxis;
                //        double halfAngle = Math.Acos(rot[counter * 4]);
                //        double X = rot[counter * 4 - 3] / Math.Sin(halfAngle);
                //        double Y = rot[counter * 4 - 2] / Math.Sin(halfAngle);
                //        double Z = rot[counter * 4 - 1] / Math.Sin(halfAngle);
                //        if (HasValidValue(X) && HasValidValue(Y) && HasValidValue(Z))
                //        {
                //            rotationAxis = new Vector3d(X, Y, Z);
                //        }

                //        double angle = 2 * halfAngle;

                //        Transform translation = Transform.Translation(translations.get_DataItem(path, j).Value);
                //        Transform rotation = Transform.Rotation(angle, rotationAxis, new Point3d(center_X, center_Y, center_Z));

                //        Transform t = translation * rotation;

                //        // Start to append data and transform rigid body geometry
                //        rotateAxis.Append(new GH_Vector(rotationAxis), path);
                //        rotateAngles.Append(new GH_Number(angle), path);
                //        transformations.Append(new GH_Transform(t), path);

                //        if (applyTransformation)
                //        {
                //            IGH_GeometricGoo transformedGeo = geos.get_DataItem(path, j).Transform(t);
                //            outputGeos.Insert(transformedGeo, path, j);
                //        }
                //    }
                //}

                for (int i = 0; i < round; i++)
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

                    if (applyTransformation && geo_list.Count > i)
                    {
                        // Apply transformation
                        geo_list[i].Transform(t);
                    }
                }
            }

            DA.SetDataList("Translations", translations);
            DA.SetDataList("Rotation Axis", rotateAxis);
            DA.SetDataList("Rotation Angles", rotateAngles);
            DA.SetDataList("Transformations", transformations);

            //DA.SetDataList(4, geo_list);

            GH_Structure<IGH_GeometricGoo> outputGeos = new GH_Structure<IGH_GeometricGoo>();
            int counter = -1;
            for (int i = 0; i < groupCount.Count; i++)
            {
                GH_Path path = new GH_Path(i);
                for (int j = 0; j < groupCount[i]; j++)
                {
                    counter++;
                    outputGeos.Append(GH_Convert.ToGeometricGoo(geo_list[counter]), path);
                }
            }
            DA.SetDataTree(4, outputGeos);

            //DA.SetDataTree(0, translations);
            //DA.SetDataTree(1, rotateAxis);
            //DA.SetDataTree(2, rotateAngles);
            //DA.SetDataTree(3, transformations);
            //DA.SetDataTree(4, outputGeos);
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
            get { return new Guid("FF4711A5-03D7-4B10-BA89-F18775C4044F"); }
        }
    }
}