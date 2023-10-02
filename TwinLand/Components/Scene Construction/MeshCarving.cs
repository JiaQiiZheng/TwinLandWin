using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace TwinLand.Components.Scene_Construction
{
    public class MeshCarving : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the MeshCarving class.
        /// </summary>
        public MeshCarving()
          : base("Mesh Carving", "mesh carving",
              "Carve mesh according to input boundary and selected algorithm.", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundary", "boundary", "", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Sinking", "sinking", "", GH_ParamAccess.item, 0);

            pManager[2].Optional = true;
            pManager[0].DataMapping = GH_DataMapping.Flatten;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "mesh", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> boundaries = new List<Curve>();
            Mesh mesh = new Mesh();
            double sinking = 0;

            if (!DA.GetDataList("Boundary", boundaries) || boundaries[0] == null) { return; }
            if (!DA.GetData("Mesh", ref mesh)) { return; }
            DA.GetData("Sinking", ref sinking);

            List<Point3d> vertices = new List<Point3d>();
            List<MeshFace> faces = new List<MeshFace>();
            List<Color> colors = new List<Color>();

            int v_count = mesh.Vertices.Count;
            int f_count = mesh.Faces.Count;
            int c_count = mesh.VertexColors.Count;

            for (int i = 0; i < v_count; i++)
            {
                vertices.Add(mesh.Vertices[i]);
            }

            for (int i = 0; i < f_count; i++)
            {
                faces.Add(mesh.Faces[i]);
            }

            if (c_count > 0)
            {
                for (int i = 0; i < c_count; i++)
                {
                    colors.Add(mesh.VertexColors[i]);
                }
            }


            /// mesh carving process
            // check if vertices are inside of any curve

            List<PointContainment> pt_containments = new List<PointContainment>();
            for (int i = 0; i < boundaries.Count; i++)
            {
                Plane pl = new Plane();
                bool xy = boundaries[i].TryGetPlane(out pl);
                if (!xy) { pl = Plane.WorldXY; }

                for (int j = 0; j < vertices.Count; j++)
                {
                    int ptc = (int)(boundaries[i].Contains(vertices[j], pl, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance));

                    // 0->unset
                    // 1->inside
                    // 2->outside
                    // 3->coincident
                    if (ptc == 1)
                    {
                        // move pts inside indicate area
                        // TODO: algorithm developing to simulate feature variation in different materials
                        Point3d pt_old = vertices[j];
                        Point3d pt_new = new Point3d(pt_old.X, pt_old.Y, pt_old.Z -= sinking);
                        vertices[j] = pt_new;
                    }
                }
            }

            // construct new mesh
            Mesh mesh_new = new Mesh();
            for (int i = 0; i < v_count; i++)
            {
                mesh_new.Vertices.Add(vertices[i]);
            }
            for (int i = 0; i < f_count; i++)
            {
                mesh_new.Faces.AddFace(faces[i]);
            }
            if (c_count > 0)
            {
                for (int i = 0; i < c_count; i++)
                {
                    mesh_new.VertexColors.Add(colors[i]);
                }
            }

            mesh_new.FaceNormals.ComputeFaceNormals();
            mesh_new.Normals.ComputeNormals();
            mesh_new.Compact();

            DA.SetData("Mesh", mesh_new);
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
            get { return new Guid("0A18B209-725E-440E-A31A-BA0D1BA2037D"); }
        }


        /// reference
        /// 
        // mesh deconstruct
        // https://discourse.mcneel.com/t/how-to-efficiently-extract-mesh-data/106137

        // mesh construct
        // https://discourse.mcneel.com/t/creating-meshes-given-points-and-their-connectivity-c/83731
    }
}