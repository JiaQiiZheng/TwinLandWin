using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TwinLand
{
    public class FleX_Engine : TwinLandComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FleX_Engine()
          : base("FleX_Engine", "FleX_Engine",
            "Core solver of FleX engine", "Solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameters", "Parameters", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Colliders", "Colliders", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force Fields", "Force Fields", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Scenes", "Scenes", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Constraints", "Constraints", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Solver Options", "Solver Options", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Run", "Run", "", GH_ParamAccess.item, false);

            for (int i = 0; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
                if (i == 3 || i == 4)
                {
                    pManager[i].DataMapping = GH_DataMapping.Flatten;
                }
            }
        }



        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FleX", "FleX", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("FleX_Log", "FleX_Log", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// initialize time related values
        /// </summary>
        Flex flex = null;
        int counter = 0;
        Stopwatch sw_1 = new Stopwatch();
        Stopwatch sw_2 = new Stopwatch();
        long totalTime_ms = 0;
        long totalUpdateTime_ms = 0;
        List<string> log = new List<string>();

        // time stamps
        int options_ts = 0;
        int param_ts = 0;
        List<int> scene_tss = new List<int>();
        List<int> constraint_tss = new List<int>();
        List<int> forceField_tss = new List<int>();
        int collider_ts = 0;

        Task<int> UpdateTask;

        /// <summary>
        /// static method
        /// </summary>
        /// <returns></returns>
        private int Update()
        {
            sw_2.Restart();
            flex.UpdateSolver();
            return (int)sw_2.ElapsedMilliseconds;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            UpdateTask = new Task<int>(() => Update());

            FlexParams param = new FlexParams();
            FlexCollisionGeometry collisionGeometry = new FlexCollisionGeometry();
            List<FlexForceField> forceFields = new List<FlexForceField>();
            List<FlexScene> scenes = new List<FlexScene>();
            List<ConstraintSystem> constraints = new List<ConstraintSystem>();
            FlexSolverOptions options = new FlexSolverOptions();

            bool reset = false;
            bool run = false;

            DA.GetData("Reset", ref reset);
            DA.GetData("Run", ref run);

            if (reset)
            {
                // reset time tracking
                counter = 0;
                totalTime_ms = 0;
                totalUpdateTime_ms = 0;
                sw_1.Stop();
                sw_2.Reset();

                log = new List<string>();

                // refresh input
                DA.GetData("Parameters", ref param);
                DA.GetData("Colliders", ref collisionGeometry);
                DA.GetDataList("Force Fields", forceFields);
                DA.GetDataList("Scenes", scenes);
                DA.GetDataList("Constraints", constraints);
                DA.GetData("Solver Options", ref options);

                scene_tss = new List<int>();
                forceField_tss = new List<int>();


                // destroy all FleX instance
                if (flex != null)
                {
                    flex.Destroy();
                }


                // create new instance and reassign values
                flex = new Flex();

                flex.SetParams(param);
                flex.SetCollisionGeometry(collisionGeometry);
                flex.SetForceFields(forceFields);

                foreach (FlexForceField f in forceFields)
                {
                    forceField_tss.Add(f.TimeStamp);
                }

                FlexScene scene = new FlexScene();
                foreach (FlexScene s in scenes)
                {
                    scene.AppendScene(s);
                    scene_tss.Add(s.TimeStamp);
                }

                foreach (ConstraintSystem cs in constraints)
                {
                    scene.RegisterCustomConstraints(cs.AnchorIndices, cs.ShapeMatchingIndices, cs.ShapeStiffness, cs.SpringPairIndies, cs.SpringStiffness, cs.SpringDefaultLengths, cs.TriangleIndices, cs.TriangleNormals);

                    constraint_tss.Add(cs.TimeStamp);
                }

                // set up scene and options for FleX engine
                flex.SetScene(scene);
                flex.SetSolverOptions(options);
            }
            
            else if (run && flex != null && flex.IsReady())
            {
                DA.GetData("Solver Options", ref options);
                if(options.TimeStamp != options_ts)
                {
                    flex.SetSolverOptions(options);
                }

                if (options.SceneMode == 0 || options.SceneMode == 1)
                {
                    // update params if timeStamp expired
                    DA.GetData("Parameters", ref param);
                    if (param.TimeStamp != param_ts)
                    {
                        flex.SetParams(param);
                        param_ts = param.TimeStamp;
                    }

                    // update colliders if timeStamp expired
                    if (DA.GetData("Colliders", ref collisionGeometry))
                    {
                        if (collisionGeometry.TimeStamp != collider_ts)
                        {
                            flex.SetCollisionGeometry(collisionGeometry);
                            collider_ts = collisionGeometry.TimeStamp;
                        }
                    }
                    else if (collisionGeometry != null)
                    {
                        flex.SetCollisionGeometry(new FlexCollisionGeometry());
                    }

                    // update forceFields if timeStamp expired
                    DA.GetDataList("Force Fields", forceFields);
                    bool needUpdate = false;
                    for (int i = forceField_tss.Count; i < forceFields.Count; i++)
                    {
                        forceField_tss.Add(forceFields[i].TimeStamp);
                        needUpdate = true;
                    }
                    for (int i = 0; i < forceFields.Count; i++)
                    {
                        if (forceFields[i].TimeStamp != forceField_tss[i])
                        {
                            needUpdate = true;
                            forceField_tss[i] = forceFields[i].TimeStamp;
                        }
                    }
                    if (needUpdate)
                    {
                        flex.SetForceFields(forceFields);
                    }

                    // update scenes if timeStamp expired
                    DA.GetDataList("Scenes", scenes);
                    for (int i = scene_tss.Count; i < scenes.Count; i++)
                    {
                        scene_tss.Add(scenes[i].TimeStamp);
                    }
                    for (int i = 0; i < scenes.Count; i++)
                    {
                        if (scenes[i].TimeStamp != scene_tss[i])
                        {
                            if (options.SceneMode == 0)
                            {
                                flex.SetScene(flex.Scene.AlterScene(scenes[i], false));
                            }
                            else
                            {
                                flex.SetScene(flex.Scene.AppendScene(scenes[i]));
                                scene_tss[i] = scenes[i].TimeStamp;
                            }
                        }
                    }

                    // update constraint if timeStamp expired
                    DA.GetDataList("Constraints", constraints);
                    for (int i = constraint_tss.Count; i < constraints.Count; i++)
                    {
                        constraint_tss.Add(constraints[i].TimeStamp);
                    }
                    for (int i = 0; i < constraints.Count; i++)
                    {
                        ConstraintSystem cs = constraints[i];
                        if (cs.TimeStamp != constraint_tss[i])
                        {
                            if (!flex.Scene.RegisterCustomConstraints(cs.AnchorIndices, cs.ShapeMatchingIndices, cs.ShapeStiffness, cs.SpringPairIndies, cs.SpringStiffness, cs.SpringDefaultLengths, cs.TriangleIndices, cs.TriangleNormals))
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Constraints indices exceeded particle count. No constraints applied.");
                            }
                            // if failed, scene remains the same
                            flex.SetScene(flex.Scene);
                            constraint_tss[i] = constraints[i].TimeStamp;
                        }
                    }
                }

                // logging
                log = new List<string>();
                counter++;
                log.Add(counter.ToString());
                long timer_ms = sw_1.ElapsedMilliseconds;
                sw_1.Restart();
                totalTime_ms += timer_ms;
                log.Add(totalTime_ms.ToString());
                log.Add(timer_ms.ToString());
                float average = (float)totalTime_ms / (float)counter;
                log.Add(average.ToString());

                // start update
                UpdateTask.Start();

                // add solver timing info to log
                int timerSolver = UpdateTask.Result;
                totalTime_ms += timerSolver;
                float ratUpdateTime = (float)totalUpdateTime_ms / (float)counter;
                log.Add(timerSolver.ToString());
                log.Add(ratUpdateTime.ToString());
            }

            if (run && options.FixedTotalIterations < 1)
            {
                ExpireSolution(true);
            }
            else if (flex != null && UpdateTask.Status == TaskStatus.Running)
            {
                UpdateTask.Dispose();
            }
            if (flex != null)
            {
                DA.SetData("FleX", flex);
            }
            DA.SetDataList("FleX_Log", log);
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
                return Properties.Resources.TL_Engine;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ceffff54-1106-4f0d-9cfe-2345c5b193f4"); }
        }
    }
}