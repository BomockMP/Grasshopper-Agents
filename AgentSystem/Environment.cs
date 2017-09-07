using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace AgentSystem
{
   public class Environment
    {

        public List<Agent> pop;
        public List<Agent> removeAgents;
        public List<Agent> addAgents;

        public int testing = 0;


        //octree
        public PointOctree pts;
        public double bounds;
        public AABB boundary;

        //container (boundary created from planes)

        public List<Plane> container;

        //constructor
        public Environment(double _bounds, List<Plane> _container)
        {

            pop = new List<Agent>();
            removeAgents = new List<Agent>();
            addAgents = new List<Agent>();
            bounds = _bounds;
            pts = new PointOctree(null, new Vector3d(-bounds, -bounds, -bounds), bounds * 2);
            container = _container;

            //note - we might not use this once planes implemented
            //construct generic bbox
            boundary = new AABB(new Vector3d(-bounds, -bounds, -bounds), new Vector3d(bounds * 2, bounds * 2, bounds * 2));
            //reconstruct to specific bounds
            boundary = boundary.fromMinMax(new Vector3d(-bounds, -bounds, -bounds), new Vector3d(bounds * 2, bounds * 2, bounds * 2));



        }

        //functions

        public void run()
        {



            if (pop.Count() > 0)
            {

                //Rhino.RhinoApp.WriteLine(pop.ElementAt(2).position.ToString());

                foreach (var a in pop)
                {
                    a.run(this);
                }

            }
        }

        //update environment function
        public void update()
        {

            //add agents in these lists

            if (addAgents.Count() > 0)
            {
                foreach (var a in addAgents) { pop.Add(a); }
            }

            foreach (var a in removeAgents) { pop.Remove(a); }

            //clear lists
            addAgents.Clear();
            removeAgents.Clear();

            //octree
            if (pop.Count() > 0)
            {

                pts = new PointOctree(null, new Vector3d(-bounds, -bounds, -bounds), bounds * 2);
                foreach (var a in pop)
                {
                    pts.addPoint(a.position);
                }
            }





        }


        //function for adding plane list

        public void setPlanes(List<Plane> planeList)
        {
            container.Clear();

            if (planeList.Count() > 0)
            {

                container = planeList;
            }
        }


              public List<Plane> getPlanes()
        {
            

            //if (container.Count() > 0)
            //{

                return container;
           // } else
           // {

                
          //  }



        }

        //-----------------------------------------
        //Functions for creating and removing Agents
        //--------------------------------------------

        public void addAgentList(List<Agent> agents)
        {

            foreach (var a in agents) { addAgent(a); }

        }

        public void addAgent(Agent a)
        {
            addAgents.Add(a);
        }


        public void removeAgent(Agent a)
        {
            removeAgents.Add(a);
        }

        public void removeAllAgents()
        {
            removeAgents.AddRange(pop);
        }

        //octree function

        public List<Vector3d> getWithinSphere(Vector3d p, float radius)
        {

            return pts.getPointsWithinSphere(p, radius);
        }
        //
        //octree function

        public List<Agent> getAgentsWithinSphere(Vector3d p, float radius)
        {

            List<Vector3d> positionList = new List<Vector3d>();
            List<Agent> agentList = new List<Agent>();
            //list of positions
            positionList = pts.getPointsWithinSphere(p, radius);
            //for each vector position, find the agent

            foreach (var pos in positionList)
            {
                //check if position is equal to that of agents pos

                foreach (var agent in pop)
                {

                    if (agent.position == pos && agent.position != p)
                    {
                        agentList.Add(agent);
                        //add agent to return list

                    }

                }
            }

            return agentList;
        }


    

}
}
