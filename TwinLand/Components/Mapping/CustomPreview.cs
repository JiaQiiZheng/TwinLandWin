using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using GH_IO.Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using OSGeo.OGR;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using Rhino.UI.Forms;

namespace TwinLand.Components.Mapping
{
    public class CustomPreview : TwinLandComponent
    {
        /// <summary>
        /// Initializes a new instance of the CustomPreview class.
        /// </summary>
        public CustomPreview()
          : base("TwinLand Preview", "TL preview",
              "", "Mapping")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "geometry", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Thickness", "thinkness", "", GH_ParamAccess.item, 1);
            pManager.AddGenericParameter("Material", "material", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "geometry", "", GH_ParamAccess.item);
            pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_GeometricGoo geo = null;
            int thinkness = 1;
            Object material = null;
            GH_Colour color = new GH_Colour(Color.Black);

            if (!DA.GetData("Geometry", ref geo)) { return; }
            DA.GetData("Thickness", ref thinkness);
            if (!DA.GetData("Material", ref material)) { return; }

            _clip = BoundingBox.Union(_clip, geo.Boundingbox);

            DisplayMaterial displayMaterial = new DisplayMaterial();

            if (material != null)
            {
                if (material.GetType().Name == color.GetType().Name)
                {
                    color = (GH_Colour)material;
                    displayMaterial.Diffuse = color.Value;
                    displayMaterial.Transparency = 1.0-((double)1 / 255 * color.Value.A);
                    _displayMaterials.Add(new DisplayMaterial(displayMaterial.Diffuse, displayMaterial.Transparency));
                }

                else if (material.GetType().Name == "GH_Material")
                {
                    GH_Material gh_material = (GH_Material)material;
                    displayMaterial = gh_material.Value;
                    _displayMaterials.Add(displayMaterial);
                }
            }

            if (geo.GetType().Name == "GH_Curve")
            {
                GH_Curve gh_crv = geo as GH_Curve;
                _curves.Add(gh_crv.Value);
            }

            else if (geo.GetType().Name == "GH_Line")
            {
                Grasshopper.Kernel.Types.GH_Line l = (Grasshopper.Kernel.Types.GH_Line)geo;
                _lines.Add(l.Value);
            }
            
            else if(geo.GetType().Name == "GH_Box")
            {
                GH_Box b = (GH_Box)geo;
                Brep brep = Brep.CreateFromBox(b.Value);
                _breps.Add(brep);
            }

            else if (geo.GetType().Name == "GH_Brep")
            {
                GH_Brep gh_brep = (GH_Brep)geo;
                _breps.Add(gh_brep.Value);
            }

            else if (geo.GetType().Name == "GH_Mesh")
            {
                GH_Mesh gh_mesh = (GH_Mesh)geo;
                _meshes.Add(gh_mesh.Value);
            }

            _thickness.Add(thinkness);

            DA.SetData("Geometry", geo);
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
                return Properties.Resources.T_icon;
            }
        }

        /// <summary>
        /// Dynamic values
        /// </summary>
        private BoundingBox _clip;
        private readonly List<Curve> _curves = new List<Curve>();
        private readonly List<Line> _lines = new List<Line>();
        private readonly List<Brep> _breps = new List<Brep>();
        private readonly List<Rhino.Geometry.Mesh> _meshes = new List<Rhino.Geometry.Mesh>();
        private readonly List<int> _thickness = new List<int>();
        private readonly List<DisplayMaterial> _displayMaterials = new List<DisplayMaterial>();

        /// <summary>
        /// Additional Methods
        /// </summary>
        protected override void BeforeSolveInstance()
        {
            _clip = BoundingBox.Empty;
            _curves.Clear();
            _lines.Clear();
            _breps.Clear();
            _meshes.Clear();
            _thickness.Clear();
            _displayMaterials.Clear();
        }

        public override BoundingBox ClippingBox
        {
            get { return _clip; }
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);

            if (!this.Attributes.Selected)
            {
                try
                {
                    for (int i = 0; i < _curves.Count; i++)
                    {
                        args.Display.DrawCurve(_curves[i], _displayMaterials[i].Diffuse, _thickness[i]);
                    }

                    for (int i = 0; i < _lines.Count; i++)
                    {
                        args.Display.DrawLine(_lines[i], _displayMaterials[i].Diffuse, _thickness[i]);
                    }

                    for (int i = 0; i < _breps.Count; i++)
                    {
                        args.Display.DrawBrepShaded(_breps[i], _displayMaterials[i]);
                    }

                    for (int i = 0; i < _meshes.Count; i++)
                    {
                        args.Display.DrawMeshShaded(_meshes[i], _displayMaterials[i]);
                    }
                }
                catch (Exception)
                {
                    //AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Geometry has null value could not preview.");
                }
            }

            else
            {
                try
                {
                    for (int i = 0; i < _curves.Count; i++)
                    {
                        args.Display.DrawCurve(_curves[i], args.WireColour_Selected, args.DefaultCurveThickness);
                    }

                    for (int i = 0; i < _lines.Count; i++)
                    {
                        args.Display.DrawLine(_lines[i], args.WireColour_Selected, args.DefaultCurveThickness);
                    }

                    for (int i = 0; i < _breps.Count; i++)
                    {
                        args.Display.DrawBrepShaded(_breps[i], args.ShadeMaterial_Selected);
                    }

                    for (int i = 0; i < _meshes.Count; i++)
                    {
                        args.Display.DrawMeshShaded(_meshes[i], args.ShadeMaterial_Selected);
                    }
                }
                catch (Exception)
                {
                    //AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Geometry has null value could not preview.");
                }
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("41EBB7C0-9DCE-402B-9106-E469F7C8BB2B"); }
        }
    }
}