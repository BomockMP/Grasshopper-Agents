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

          

            //Agent Params #3 - #8
            //Agent force strengths
            pManager.AddNumberParameter("Agent Align Strength", "align str", "force to be applied to agent align behaviour as a double", GH_ParamAccess.item);
            pManager.AddNumberParameter("Agent attraction Strength", "attract str", "force to be applied to agent attraction or cohesion behaviour as a double", GH_ParamAccess.item);
            pManager.AddNumberParameter("Agent repel Strength", "repel str", "force to be applied to agent repel behaviour as a double", GH_ParamAccess.item);

            //agent radiuses
            pManager.AddNumberParameter("Agent Align Radius", "align rad", "radius for align behaviour to be in effect", GH_ParamAccess.item);
            pManager.AddNumberParameter("Agent attraction Radius", "attract rad", "radius for attract behaviour to be in effect", GH_ParamAccess.item);
            pManager.AddNumberParameter("Agent repel Radius", "repel rad", "radius for repel behaviour to be in effect", GH_ParamAccess.item);

            //boundary affects
            pManager.AddNumberParameter("Boundary repel strength", "boundary rad", "force to be applied to agent to repel from boundaries as a double", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bpundary repel radius", "bounds rad", "radius for repel force from boundaries to be applied to agents", GH_ParamAccess.item);


            //restart
            pManager.AddBooleanParameter("restart simulation", "restart", "toogle to restart simulation", GH_ParamAccess.item);

            //view single agents behaviour & neighbours
            pManager.AddNumberParameter("selected Agent", "selected agent", "grabs agent from list to view its neighbours, their position & velocity,", GH_ParamAccess.item);

            //alter agent speed limit and accell limit
            pManager.AddNumberParameter("agent speed limit", "agent speed", "change the limit of agents max speed", GH_ParamAccess.item);
            pManager.AddNumberParameter("agent acc limit", "agent acc", "change the limit of the agents max accel", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddVectorParameter("Agent Velocity Vector", "Agent Vel", "Vector showing agents velocity", GH_ParamAccess.list);
            pManager.AddPointParameter("Agent Location Point3d", "Agent pos", "point3d with Agents current position", GH_ParamAccess.list);

            //outputs for agent rendering. first two selected agent position and velocity. second two are lists of neighbours pos and vel.
            pManager.AddVectorParameter("Selected Agent Location Vector", "Selected Agent pos", "Vector3d with Agents current position", GH_ParamAccess.item);
            pManager.AddVectorParameter("Selected Agent velocity Vector", "Selected Agent vel", "Vector3d with Agents current velocity", GH_ParamAccess.item);
            pManager.AddVectorParameter("Agents Neighbours Positions", "Selected Agent neighbours pos", "Vector3d withneighbouring agents positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Agents Neighbours velocity vectors", "Selected Agent neighbours vel", "Vector3d withneighbouring agents velocity vector", GH_ParamAccess.list);

            pManager.AddLineParameter("Agents vectors", "Selected line vel", "vector as line", GH_ParamAccess.list);

        }

        //--------------------------------------------------------------------------------------------------------------------
        // -------SETUP----------
        // -------------------------------------------------------------------------------------------------------------------- 

        //Global Variables
        public Boolean initialise = true;
        public Environment environment = new Environment(2000, new List<Plane>());

        //render Lists
        public List<Point3d> renderPtList = new List<Point3d>();
        public List<Vector3d> agentVelList = new List<Vector3d>();

        //neighbour rendering
        public List<Vector3d> renderPosListOfNeighboursToAgent = new List<Vector3d>();
        public List<Vector3d> renderVelListOfNeighboursToAgent = new List<Vector3d>();
        public Vector3d singleAgentPosToRender = new Vector3d();
        public Vector3d singleAgentVelToRender = new Vector3d();

        public List<Line> agentForceLineList = new List<Line>();

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
            agentForceLineList.Clear();
            if (environment.pop.Count() > 0)
            {

                foreach (var a in environment.pop)
                {
                    Point3d pt = new Point3d(a.position.X, a.position.Y, a.position.Z);
                    renderPtList.Add(pt);
                    Vector3d agentVel = a.vel;
                    agentVelList.Add(agentVel);

                    agentForceLineList.Add(a.lineOfForce);
                }
            }
        }


        public void renderNeighbouringAgents(double agentToRender) {
            //function to select an agent from list based on the agentToRender number imput.
            //take that agent and find its neighbours.
            //Add its neighbours to an array that is refreshed each frame
            //output these neighbours position vector3ds as a list * their velocity vectors as a list
            //output the selected agents position & velocity

            //Clear Lists
            renderPosListOfNeighboursToAgent.Clear();
            renderVelListOfNeighboursToAgent.Clear();

            //if the agent list is populated and the selected agent is possible in the list
            if (environment.pop.Count() > 0 && agentToRender < environment.pop.Count())
            {
                //make a variable for the selected agent
               Agent selectedAgent = environment.pop.ElementAt((int)agentToRender);

                //add selected agents vel and pos to global variable
                singleAgentPosToRender = selectedAgent.position;
                singleAgentVelToRender = selectedAgent.vel;

        //make a temp list of neighbours
                List<Agent> neighbouringAgents = new List<Agent>();

                //fill it with the agents neighbours. This will now be a list of agents. check it has neighbours first.
                //Rhino.RhinoApp.WriteLine(selectedAgent.neighbourAgents.Count().ToString());


                if (selectedAgent.neighbours.Count() > 0) {
                neighbouringAgents = selectedAgent.neighbours;

                //fill the relevant lists. For each agent in the neighouring list, get its position vec and velocity vec and add to lists
                foreach (var a in neighbouringAgents) {
                    renderPosListOfNeighboursToAgent.Add(a.position);
                    renderVelListOfNeighboursToAgent.Add(a.vel);

                }
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
            Double agentSearchRadius = 0;
            BoundingPlaneList = new List<Plane>();

            Double boundaryRepelStrength = 0;
            Double boundaryRepelRadius = 0;

            //Agent behavior strengths & radius

            Double agentAlignStrength = 0;
            Double agentAttractStrength = 0;
            Double agentRepelStrength = 0;

            Double agentAlignRadius = 0;
            Double agentAttractRadius = 0;
            Double agentRepelRadius = 0;


            Boolean restart = false;

            //Selected agent out of list for viewing behaviours & neighbours
            Double selectedAgent = 0;


            double agentSpeed = 0;
            double agentAcc = 0;
            //boundaryRepelStrength = _boundaryRepelStrength;
            // boundaryRepelRadius = _boundaryRepelRadius;


            //2. retrieve imput data

            if (!DA.GetDataList(0, spawnPtList)) { return; }
 if (!DA.GetData(1, ref agentSearchRadius)) { return; }
 if (!DA.GetDataList(2, BoundingPlaneList)) { return; }


            

            //Agent behaviour imput data



            if (!DA.GetData(3, ref agentAlignStrength)) { return; }
            if (!DA.GetData(4, ref agentAttractStrength)) { return; }
            if (!DA.GetData(5, ref agentRepelStrength)) { return; }
            if (!DA.GetData(6, ref agentAlignRadius)) { return; }
            if (!DA.GetData(7, ref agentAttractRadius)) { return; }
            if (!DA.GetData(8, ref agentRepelRadius)) { return; }


            if (!DA.GetData(9, ref boundaryRepelStrength)) { return; }
            if (!DA.GetData(10, ref boundaryRepelRadius)) { return; }


            if (!DA.GetData(11, ref restart)) { return; }

            //select agent
            if (!DA.GetData(12, ref selectedAgent)) { return; }

            //agent speed and acc limits
            if (!DA.GetData(13, ref agentSpeed)) { return; }
            if (!DA.GetData(14, ref agentAcc)) { return; }

            //3. abort on invalid inputs

            //4. 

            if (restart)
            {
                Rhino.RhinoApp.WriteLine("Restart");
                initialise = true;
                restart = false;
            }

            //setup code - to run once only
            if (initialise){


                environment.setPlanes(BoundingPlaneList);

                environment.removeAllAgents();

      if(spawnPtList.Count() > 0){
        for (int i = 0; i < spawnPtList.Count(); i++) {


          Point3d spawnPosition = spawnPtList.ElementAt(i);

                        //alt constructor
                        //Agent a = new Agent(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, agentSearchRadius); 
                        // double agentSpeed = 0;
                       // double agentAcc = 0;
                        Agent a = new Agent(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, agentSearchRadius, agentAlignStrength, agentAttractStrength, agentRepelStrength, agentAlignRadius, agentAttractRadius, agentRepelRadius, boundaryRepelStrength, boundaryRepelRadius, agentSpeed, agentAcc);

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
    renderNeighbouringAgents(selectedAgent);

            //5.)
            //assign outputs
            DA.SetDataList(0, agentVelList);
            DA.SetDataList(1, renderPtList);

            //create output for selected agent position & velocity (item vector3d) & selected agents neighbours position & velocity (list vector3d)
            DA.SetData(2, singleAgentPosToRender);
            DA.SetData(3, singleAgentVelToRender);
            DA.SetDataList(4, renderPosListOfNeighboursToAgent);
            DA.SetDataList(5, renderVelListOfNeighboursToAgent);

            DA.SetDataList(6, agentForceLineList);


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
