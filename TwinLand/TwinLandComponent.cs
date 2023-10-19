using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TwinLand
{
    public abstract class TwinLandComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TwinLandComponent(string name, string nickName, string description, string subCategory)
          : base(name, nickName,
            description, "TwinLand", subCategory)
        {
        }
        
        public class ParamCombo
        {
            public string paramName { get; set; }
            public object paramValue { get; set; }
            public string paramType { get; set; }

            public ParamCombo(string paramName, object paramValue)
            {
                this.paramName = paramName;
                this.paramValue = paramValue;
                this.paramType = paramValue.GetType().ToString();
            }
        }
    }
}