using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.IO.PBF;
using Rhino.Geometry;
using Rhino.Render.DataSources;

namespace TwinLand
{
    public class FilterOpenStreetMap : TwinLandComponent, IGH_VariableParameterComponent, IDisposable
    {
        Dictionary<string, Type> uniqueChildProperties;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FilterOpenStreetMap()
          : base("Filter OpenStreetMap", "Filter OSM",
            "Filter OpenStreetMap by input 'Key=Value' expression", "Data Engineering")
        {
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("FeatureGeometry", "featureGeometry", "Feature geometry from OpenStreetMap file",
              GH_ParamAccess.tree);
            pManager.AddTextParameter("Values", "values", "Data value list of each feature geometry", GH_ParamAccess.tree);
            pManager.AddTextParameter("OSM_Tag_Key = Value", "OSM_Tag_Key = Value",
              "String value used to filter the osm data, format like 'natural=water'", GH_ParamAccess.tree);

            pManager[2].DataMapping = GH_DataMapping.Graft;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Filtered Collection", "filtered", "Filtered Collection of feature geometries", GH_ParamAccess.tree);

            // dynamic add output features match to filter keys
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_GeometricGoo> geoFeatures = new GH_Structure<IGH_GeometricGoo>();
            DA.GetDataTree("FeatureGeometry", out geoFeatures);

            GH_Structure<GH_String> values = new GH_Structure<GH_String>();
            DA.GetDataTree("Values", out values);

            GH_Structure<GH_String> matchExpressions = new GH_Structure<GH_String>();
            DA.GetDataTree("OSM_Tag_Key = Value", out matchExpressions);

            // declare a new data tree to contain features
            GH_Structure<IGH_GeometricGoo> newGeo = new GH_Structure<IGH_GeometricGoo>();

            for (int i = 0; i < matchExpressions.PathCount; i++)
            {
                GH_Path path = matchExpressions.get_Path(i);
                var branch = matchExpressions.get_Branch(path);

                for (int j = 0; j < branch.Count; j++)
                {
                    // split the expression to match key
                    string matchKey = branch[j].ToString().Split('=')[1];
                    var singleKeyPath = path.AppendElement(j);

                    // loop through all features and put match features from same key into the same root path
                    bool isMatch = false;

                    for (int k = 0; k < values.Branches.Count(); k++)
                    {
                        List<GH_String> valueList = values[k];
                        for (int l = 0; l < valueList.Count; l++)
                        {

                            if (valueList[l].Value.Equals(matchKey))
                            {
                                isMatch = true;
                                foreach (var geo in geoFeatures[k])
                                {
                                    newGeo.Append(geo, singleKeyPath);
                                }
                            }
                        }
                    }

                    // add null to the key path if not match
                    if (!isMatch)
                    {
                        newGeo.Append(null, singleKeyPath);
                    }
                }
            }


            List<string> addedExpressions = new List<string>();

            // register all output param when compute at first time
            if (this.Params.Output.Count == 1)
            {
                for (int i = 0; i < newGeo.Branches.Count; i++)
                {
                    string expression = matchExpressions.Branches[i][0].Value;
                    addedExpressions.Add(expression);

                    int paramIndex = newGeo.get_Path(i)[1] + 1;

                    var param = CreateParameter(GH_ParameterSide.Output, this.Params.Output.Count);
                    param.Name = expression;
                    param.NickName = expression;
                    param.Access = GH_ParamAccess.list;
                    this.Params.RegisterOutputParam(param, paramIndex);

                    this.Params.OnParametersChanged();
                    ExpireSolution(true);
                }
            }

            // add output params
            else if (newGeo.Branches.Count >= this.Params.Output.Count)
            {
                for (int i = 0; i < newGeo.Branches.Count; i++)
                {
                    string checkExpression = matchExpressions.Branches[i][0].Value;

                    // check whether it's existed in output params
                    bool isExist = false;
                    for (int j = 0; j < this.Params.Output.Count; j++)
                    {
                        if (checkExpression == this.Params.Output[j].Name)
                        {
                            isExist = true;
                        }
                    }

                    if (!isExist)
                    {
                        var param = CreateParameter(GH_ParameterSide.Output, this.Params.Output.Count);
                        param.Name = checkExpression;
                        param.NickName = checkExpression;
                        param.Access = GH_ParamAccess.list;
                        this.Params.RegisterOutputParam(param);

                        this.Params.OnParametersChanged();
                        ExpireSolution(true);
                    }
                }
            }

            // delete output param if input expressions do not include it anymore
            else if (newGeo.Branches.Count < this.Params.Output.Count)
            {
                List<int> deleteParamIndex = new List<int>();

                for (int i = 1; i < this.Params.Output.Count; i++)
                {
                    string checkParam = this.Params.Output[i].Name;
                    bool isExist = false;
                    // check whether it's existed in input expressions
                    for (int j = 0; j < matchExpressions.Branches.Count; j++)
                    {
                        if (checkParam == matchExpressions.Branches[j][0].Value)
                        {
                            isExist = true;
                            goto end_of_loop;
                        }

                        if (j == matchExpressions.Branches.Count - 1 && !isExist)
                        {
                            deleteParamIndex.Add(i);
                        }
                    }
                end_of_loop: { }
                }


                foreach (int paramIndex in deleteParamIndex)
                {
                    this.Params.UnregisterOutputParameter(this.Params.Output[paramIndex]);

                    this.Params.OnParametersChanged();
                    ExpireSolution(true);
                }
            }

            DA.SetDataTree(0, newGeo);

            // append all features into seperated output params
            if (this.Params.Output.Count >= 1)
            {
                for (int i = 0; i < newGeo.Branches.Count; i++)
                {
                    string expression = this.Params.Output[i + 1].Name;
                    GH_Path featurePath = new GH_Path();
                    for (int j = 0; j < matchExpressions.Branches.Count; j++)
                    {
                        if (matchExpressions.Branches[j][0].Value.Equals(expression))
                        {
                            featurePath = matchExpressions.get_Path(j);
                        }
                    }

                    try
                    {
                        DA.SetDataList(i + 1, newGeo.get_Branch(featurePath.AppendElement(0)));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.T_icon;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d1fca95e-05fa-4f75-8200-e71df714487c"); }
        }

        /// <summary>
        /// Implement the interface of IGH_VariableParameterComponent
        /// </summary>
        /// <param name="side"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject();
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            var tokens = uniqueChildProperties;
            if (tokens == null) return;
            var names = tokens.Keys.ToList();
            for (var i = 0; i < Params.Output.Count; i++)
            {
                if (i > names.Count - 1) return;
                var name = names[i];
                var type = tokens[name];

                Params.Output[i].Name = $"{name}";
                Params.Output[i].NickName = $"{name}";
                Params.Output[i].Description = $"Data from property: {name}";
                Params.Output[i].MutableNickName = false;
                if (type.IsAssignableFrom(typeof(JArray)))
                {
                    Params.Output[i].Access = GH_ParamAccess.list;
                }
                else
                {
                    Params.Output[i].Access = GH_ParamAccess.item;

                }
            }
        }

        public void Dispose()
        {
            ClearData();
            foreach (var ghParam in Params)
            {
                ghParam.ClearData();
            }
        }

        // TODO: this component could be optimized, take this as reference
        // https://github.com/andrewheumann/jSwan/blob/master/jSwan/Deserialize.cs#L286
    }
}