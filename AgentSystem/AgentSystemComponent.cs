using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace AgentSystem
{
    public class AgentSystemComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public AgentSystemComponent()
          : base("AgentSystem", "Agents",
              "Agent boid based system",
              "Extra", "Agent System")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddPointParameter("Spawn Point list", "Agent Spawn Pts", "list of points for agents to spawn at", GH_ParamAccess.list);
            pManager.AddNumberParameter("Agent Search Radius", "Search Radius", "search radius for agents to find neighbours as a double", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Bounding Planes", "Bounds", "list of planes to act as boundaries for agents", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddVectorParameter("Agent Velocity Vector", "Agent Vel", "Vector showing agents velocity", GH_ParamAccess.list);
            pManager.AddPointParameter("Agent Location Point3d", "Agent pos", "point3d with Agents current position", GH_ParamAccess.list);


        }

        //--------------------------------------------------------------------------------------------------------------------
        // -------SETUP----------
        // -------------------------------------------------------------------------------------------------------------------- 

        //Global Variables
        public Boolean initialise = true;
        public Environment environment = new Environment(500, new List<Plane>());

        //render Lists
        public List<Point3d> renderPtList = new List<Point3d>();
        public List<Vector3d> agentVelList = new List<Vector3d>();

        //Load in spawnPts for Agents
        public List<Point3d> spawnPtList = new List<Point3d>();

        //counter to keep track of iterations
        public int counter = 0;

        //bounding plane lists
        List<Plane> BoundingPlaneList = new List<Plane>();


       
        //--------------------------------------------------------------------------------------------------------------------
        // -------ADDITIONAL FUNCTIONS----------
        // -------------------------------------------------------------------------------------------------------------------- 


        public void renderAgents()
        {
            renderPtList.Clear();
            agentVelList.Clear();
            if (environment.pop.Count() > 0)
            {

                foreach (var a in environment.pop)
                {
                    Point3d pt = new Point3d(a.position.X, a.position.Y, a.position.Z);
                    renderPtList.Add(pt);
                    Vector3d agentVel = a.vel;
                    agentVelList.Add(agentVel);
                }
            }
        }

        //----------

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        /// 
        //  ------------------------------------------------------------------------------------------------------------------------------
        // DRAW LOOP ------------------------------------------------------------------------------------------------------------------------------
        //  ------------------------------------------------------------------------------------------------------------------------------


        protected override void SolveInstance(IGH_DataAccess DA)
        {

            
//1. declare placeholder variables and assign initial invalid data

            spawnPtList = new List<Point3d>();
            double agentSearchRadius = 0;
            BoundingPlaneList = new List<Plane>();
            //List<Plane> BoundingPlaneList = new List<Plane>();

            //2. retrieve imput data

            if (!DA.GetDataList(0, spawnPtList)) { return; }
 if (!DA.GetData(1, ref agentSearchRadius)) { return; }
 if (!DA.GetDataList(2, BoundingPlaneList)) { return; }


           
            //3. abort on invalid inputs

            //4. 

            //each draw loop, fill spawnptList with new data
            /*
                       if (spawnPtList.Count() > 1){
                 spawnPtList = SpawnPt;
               }
           */



            //setup code to run once
            if (initialise){


                environment.setPlanes(BoundingPlaneList);

                environment.removeAllAgents();

      if(spawnPtList.Count() > 0){
        for (int i = 0; i < spawnPtList.Count(); i++) {


          Point3d spawnPosition = spawnPtList.ElementAt(i);

          Agent a = new Agent(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, agentSearchRadius);


          environment.addAgent(a);

                        //set the bounds of the environment with a get set method one time only at the moment

                        

        }
      }


      Rhino.RhinoApp.WriteLine("Initialised");

      initialise = false;
    }

            //DRAW LOOP

            

            environment.run();


    environment.update();



    counter++;



    renderAgents();

            //5.)
            //assign outputs
            DA.SetDataList(0, agentVelList);
            DA.SetDataList(1, renderPtList);


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
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {

            //added unique GUID
            get { return new Guid("9967d54a-e257-4f3b-870f-f18fa34ade77"); }
        }
    }
}
