using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace AgentSystem
{
   public class Agent
    {
        //Class global variables

        //position variable
        public double x;
        public double y;
        public double z;
        public Vector3d position = new Vector3d();

        //Variables from Processing Particle class, may need to modify
        public Vector3d accel = new Vector3d();
        public Vector3d vel = new Vector3d();
        public double spd = 10;
        public double accLimit = 2; //less than speed for more fluidty
        public int age = 0;
        public double inertia = 1;
        public float searchRadius;
        public bool withinBounds = true;

        public List<Plane> boundingContainer = new List<Plane>();

        //List of neighbour agents. I really need these as agents not as Vector positions.
        public List<Agent> neighbours = new List<Agent>(); //change to agents
       // public List<Agent> neighbourAgents = new List<Agent>();



        //behaviour settings
        public Double agentAlignStrength = 0;
        public Double agentAttractStrength = 0;
        public Double agentRepelStrength = 0;
        public  Double agentAlignRadius = 0;
        public  Double agentAttractRadius = 0;
        public Double agentRepelRadius = 0;

        //bounds settings
        public  Double boundaryRepelStrength = 0;
        public Double boundaryRepelRadius = 50;

        //testing

        public Line lineOfForce;


        //Constructor 1

        public Agent(double _x, double _y, double _z, double _searchRadius)
        {
            x = _x;
            y = _y;
            z = _z;

            position.X = x;
            position.Y = y;
            position.Z = z;
            searchRadius = (float)_searchRadius;
            //clear list on spawning
            boundingContainer = new List<Plane>();

        }


        //constructor 2 with behaviours added
        /* 
        Double agentAlignStrength = 0;
            Double agentAttractStrength = 0;
            Double agentRepelStrength = 0;

            Double agentAlignRadius = 0;
            Double agentAttractRadius = 0;
            Double agentRepelRadius = 0;
    */
        public Agent(double _x, double _y, double _z, double _searchRadius, double _agentAlignStrength, double _agentAttractStrength, double _agentRepelStrength, double _agentAlignRadius, double _agentAttractRadius, double _agentRepelRadius, double _boundaryRepelStrength, double _boundaryRepelRadius, double _agentSpeed, double _agentAccell)
        {
            x = _x;
            y = _y;
            z = _z;

            position.X = x;
            position.Y = y;
            position.Z = z;
            searchRadius = (float)_searchRadius;
            //clear list on spawning
            boundingContainer = new List<Plane>();


            //behaviours
            agentAlignStrength = _agentAlignStrength;
            agentAttractStrength = _agentAttractStrength;
             agentRepelStrength = _agentRepelStrength;
            agentAlignRadius = _agentAlignRadius;
            agentAttractRadius = _agentAttractRadius;
            agentRepelRadius = _agentRepelRadius;

            //bounding container 
            boundaryRepelStrength = _boundaryRepelStrength;
            boundaryRepelRadius = _boundaryRepelRadius;

            //double _agentSpeed, double _agentAccell
            spd = _agentSpeed;
            accLimit = _agentAccell;
        }




        //

        //Functions to be called when running

        public void run(Environment environment)
        {

            checkContainer(environment, boundaryRepelRadius, boundaryRepelStrength);
            //checkBounds(environment, 0.002);

            if (environment.pop.Count() > 0)
            {

                //cant seem to get agents that are actually close...
                //its taking a radius from a wierd spot. not the agents location. could be origin but not quite.
                //  getNeighbours(this.position, searchRadius, environment);
                getNeighbours(searchRadius, environment);

                // getNeighbourAgents(searchRadius, environment);



                align(agentAlignRadius, agentAlignStrength);
                cohesion(agentAttractRadius, agentAttractStrength);
                seperation(agentRepelRadius, agentRepelStrength);
            }



            update();

        }

        //Functions


        //check if in plane bounding container

        public void checkContainer(Environment environment, double distanceThreshold, double strength)
        {

            //if there is currently no bounding planes in the list, get planes

            if (boundingContainer.Count() == 0)
            {
              //  Rhino.RhinoApp.WriteLine(boundingContainer.Count().ToString());
                boundingContainer = environment.getPlanes();
            }


            //boundingContainer = new List<Plane>();
            //get the planes
            //boundingContainer = environment.getPlanes();

         //   Rhino.RhinoApp.WriteLine(boundingContainer.Count().ToString());


            //if there are now planes in the list

            if (boundingContainer.Count() != 0) {

                //test the agents position against the plane. this might be too resource intensive to test every plane.
                //1) check if agent is close enough to any plane

                foreach (var p in boundingContainer) {

                    //check if plane is valid
                    if (p.IsValid) {

                        //test position of agent to plane, get the distance. if the distance
                        //is negative, the point is below the plane. 


                        double distanceToPlane = p.DistanceTo(new Point3d(position));
                        double posDistanceToPlane = distanceToPlane;

                        if (distanceToPlane <= 0) {
                            posDistanceToPlane = (distanceToPlane * -1);  }
                       

                        //if the distance is less than what is specified,
                        if (posDistanceToPlane <= distanceThreshold) {

                            //find the planes normal, which *should* will be pointing inwards from the input
                            Vector3d planeNormal = p.Normal;

                            //apply a force based on distance to the plane
                            //divide the force by the distance

                            Vector3d force = planeNormal * (strength / posDistanceToPlane);


                            addForce(force);

                          //  Rhino.RhinoApp.WriteLine(force.ToString());

                        }
                    }
                }

            }


        }


        //check if in bounds

        public void checkBounds(Environment environment, double strength)
        {

            //check if within bounds
            withinBounds = environment.boundary.containsPoint(this.position);
            //if not
            if (withinBounds == false)
            {

                //get centre of bounding box
                Vector3d BboxCentre = environment.boundary.centrePoint;
                //attract to centre

                Vector3d resultingVector = BboxCentre - this.position;
                double distToCentre = resultingVector.Length;

                resultingVector.Unitize();
                addForce(resultingVector * strength);
            }


        }
        // so agents can find each others positions using oct tree

        public void getNeighbours(float radius, Environment environment)
        {

            //clear list of current neighbours
            //

            //CHECK NEIGHBOURS STILL RELEVANT
            if (neighbours.Count() > 0) {
                Vector3d toneighbour = neighbours.ElementAt(0).position - this.position;
                double dist = toneighbour.Length;

               

                if (dist > radius) {

                    neighbours.Clear();
                    
                }

                    }
            

            if (neighbours.Count() == 0) { 

            List<Agent> addList = new List<Agent>();

            //this function in environment
            addList = environment.getWithinSphere(this, radius);
            

            if (addList != null)
            {
                neighbours.AddRange(addList);

            }
            }
        }

        /*
        public void getNeighbourAgents(Vector3d imputPosition, float radius, Environment environment)
        {

            //clear list of current neighbours
            neighbourAgents.Clear();
            List<Agent> addList = new List<Agent>();


            //this function in environment
            addList = environment.getAgentsWithinSphere(imputPosition, radius);
            //list of position vectors. The problem is, how do i know which position corresponds
            //to which agent? maybe it doesnt need to know this? try with vectors. it will only
            // know there is a vector "position" there, but assume its an agent.

            if (addList != null)
            {
                neighbourAgents.AddRange(addList);


            }
        }
        */

        //Interpolation Behaviors



        //SEPERATE
        public void seperation(Double seperationRadius, double strength)
        {


            double neighbourCount = 0;
            Vector3d computationVector = new Vector3d(0, 0, 0);



            //for each neighbour in neighbours. if neighbour != this agent,
            //if distance to agent < specified distance,

            if (neighbours.Count() > 0)
            {



                foreach (var neighbouringAgent in neighbours)
                {

                    if (neighbouringAgent != this)
                    {

                        //calculate distance to neighbours
                        //subtact the subject vector from the neighbouring vector

                        Vector3d resultingVector = neighbouringAgent.position - this.position;
                        double distToNeighbour = resultingVector.Length;




                        if (distToNeighbour <= seperationRadius)
                        {



                            //add the distance vector to the computation vector
                            computationVector += resultingVector;

                            // Rhino.RhinoApp.WriteLine(AlignRadius.ToString());

                            neighbourCount++;
                        }
                    }
                }
                //divide computation vector by the neighbour count and normalize (divide by length to get vector
                //length of 1.
                computationVector /= neighbourCount;


                computationVector *= -1;

                computationVector.Unitize();

            }
            //  return computationVector;


            // Rhino.RhinoApp.WriteLine("hello");


            addForce(computationVector * strength);



        }

        //Rhino.RhinoApp.WriteLine(strength.ToString());
        //ATTRACT
        public void cohesion(Double cohesionRadius, double strength)
        {



            double neighbourCount = 0;
            Vector3d computationVector = new Vector3d(0, 0, 0);


            if (neighbours.Count() > 0)
            {


                foreach (var neighbouringAgent in neighbours)
                {
                   // if (neighbouringAgent != this)
                    //{


                        

                        Vector3d resultingVector = neighbouringAgent.position - this.position;
                        double distToNeighbour = resultingVector.Length;

                 


                    if (distToNeighbour <= cohesionRadius && distToNeighbour > 1)
                        {

                        
                        //  computationVector += neighbouringAgent.position;
                        //=i want its position
                        computationVector = (computationVector + neighbouringAgent.position);
                            neighbourCount++;


                       // }
                    }


                }
                
                computationVector /= neighbourCount;
                //the target should be this computation vector.

                //create a vector between this agents position and the comptuation vector.

              Vector3d  desireVector = new Vector3d();

                desireVector = computationVector - position;


                //for viewing purposes
                Vector3d preview = position + (vel*10);
                Line a = new Line(new Point3d(position.X, position.Y, position.Z), new Point3d(preview.X, preview.Y, preview.Z));
                lineOfForce = a;

                desireVector.Unitize();

                desireVector = desireVector * strength;

               


                addForce(desireVector);

                //moved all inside if neighbour.count > 0 brackets. 


            }
 
        }



        //ALIGNMENT
        //Iterate through Neighbours. Add Neighbours Velocity to the computation vector
        //and increase the neighbour count to keep track

        public void align(Double AlignRadius, double strength)
        {
            double neighbourCount = 0;
            Vector3d computationVector = new Vector3d(0, 0, 0);



            //for each neighbour in neighbours. if neighbour != this agent,
            //if distance to agent < specified distance,

            if (neighbours.Count() > 0)
            {



                foreach (var neighbouringAgent in neighbours)
                {

                    if (neighbouringAgent != this)
                    {

                        //calculate distance to neighbours
                        //subtact the subject vector from the neighbouring vector

                        Vector3d resultingVector = neighbouringAgent.position - this.position;
                        double distToNeighbour = resultingVector.Length;




                        if (distToNeighbour <= AlignRadius)
                        {



                            //i dont want its position, i want its velocity
                            // computationVector += neighbouringAgent.vel;

                            computationVector += neighbouringAgent.vel;

                            // Rhino.RhinoApp.WriteLine(AlignRadius.ToString());

                            neighbourCount++;
                        }
                    }
                }
                //divide computation vector by the neighbour count and normalize (divide by length to get vector
                //length of 1.
                computationVector /= neighbourCount;
                computationVector.Unitize();

            }
            //  return computationVector;


            // Rhino.RhinoApp.WriteLine("hello");


            addForce(computationVector * strength);


        }



        //Functions for Motion (i dont think anything is updating position at the moment, force functions need to act on position)

        public void addForce(Vector3d force)
        {
            if (force.Length > 0.001)
            {
                accel = Vector3d.Add(accel, force);
            }
        }


        public void addForceAndUpdate(Vector3d force)
        {
            if (force.Length > 0.001)
            {
                accel = Vector3d.Add(accel, force);
            }
            update();
        }

        //update functions

        public void update()
        {
            update(1);
        }

        public void update(double damping)
        {
            //missing boolean - not sure if needed.
            //limit acceleration to acc limit
            double vectorLength = accel.Length;

            double limit = vectorLength / accLimit;

            if (limit != 0)
            {
                accel = Vector3d.Divide(accel, limit);
            }
            //might not work, testing
            // accel /= limit;




            //add accelleration to velocity
            if (accel.Length > 0)
            {
                Vector3d newVel = Vector3d.Add(vel, accel);
                vel = newVel;

            }




            //limit velocity to speed
            if (vel.Length > 0)
            {
                double vectorVelLength = vel.Length;


                double velLimit = vectorVelLength / spd;




                Vector3d newVel2 = Vector3d.Divide(vel, velLimit);

                //update velocity
                vel = newVel2;
            }



            //update position of agent
            position += vel;



            age++;

            //wipe accelleration vector
            accel = new Vector3d();



        }


        //set & get functions for speed,accel, inertia

        public void setSpeedLimit(double s)
        {
            spd = s;
        }


        public double getSpeed()
        {
            return vel.Length;
        }


        public void setAccelLimit(double a)
        {
            accLimit = a;
        }

        //get inertia
        //scale inertia
        //add inertia
        //set inertia



        //Render Agents - not using this atm
        public void renderAgent()
        {
            Point3d pt = new Point3d(position.X, position.Y, position.Z);
        }




        //END OF CLASS

    }
}
