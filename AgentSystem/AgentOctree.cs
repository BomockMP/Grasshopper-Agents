using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;



namespace AgentSystem
{
    public class AgentOctree : AABB
    {
        //Variables OCTREE//
        //changed all to public
        public double minNodeSize = 4; //4
        public AgentOctree parent;
        public AgentOctree[] children;
        public byte numChildren;
        public List<Agent> points;

        public double size;
        public double halfSize;
        public Vector3d offset;
        public int depth = 0;
        //private bool isAutoReducing = false;




        //constructor // :base is the same as super to pass down to AABB constructor

        public AgentOctree(AgentOctree p, Vector3d o, double halfSize) :

          base(Vector3d.Add(o, new Vector3d(halfSize, halfSize, halfSize)), new Vector3d(halfSize, halfSize, halfSize))
        {
            this.parent = p;
            this.halfSize = halfSize;
            this.size = halfSize + halfSize;
            this.offset = o;
            this.numChildren = 0;

            if (parent != null)
            {
                depth = parent.depth + 1;
                minNodeSize = parent.minNodeSize;

            }





        }



        public bool addAll(List<Agent> points)
        {

            bool addedAll = true;

            foreach (var p in points)
            {
                addedAll &= addPoint(p);
            }
            return addedAll;
        }


        public bool addPoint(Agent p)
        {
            //check if pt is inside cube - write code for contains point

            if (containsPoint(p))
            {

                if (halfSize <= minNodeSize)
                {

                    if (points == null)
                    {

                        points = new List<Agent>();

                    }

                    points.Add(p);

                    return true;

                }
                else
                {

                    Vector3d plocal = Vector3d.Subtract(p.position, offset);

                    if (children == null)
                    {
                        children = new AgentOctree[8];

                    }


                    int octant = getOctantID(plocal);
                    if (children[octant] == null)
                    {
                        Vector3d off = Vector3d.Add(offset, new Vector3d(
(octant & 1) != 0 ? halfSize : 0,
(octant & 2) != 0 ? halfSize : 0,
(octant & 4) != 0 ? halfSize : 0));

                        children[octant] = new AgentOctree(this, off, halfSize * 0.5);

                        numChildren++;

                    }
                    return children[octant].addPoint(p);

                }


            }
            return false;

        }




        public bool containsPoint(Agent p)
        {
            //    return //check whether in AABB..NOT SURE WHERE THIS CHECK SHOULD BE. IDEALLY IT IS IN AGENT TOO, BUT WE ARE WORKING WITH GENERIC VECTOR3DS HERE. SO
            // WILL DOUBLE UP FOR NOW.

            //return p.isInAABB(this);

            //check whether vector3d p is in AABB ('this', as the class is an extension of AABB. find bounds of box
            Vector3d min = this.getMin();
            Vector3d max = this.getMax();
            if (p.position.X < min.X || p.position.X > max.X) { return false; }
            if (p.position.Y < min.Y || p.position.Y > max.Y) { return false; }
            if (p.position.Z < min.Z || p.position.Z > max.Z) { return false; }

            return true;
        }

        //empty function
        public void empty()
        {
            numChildren = 0;
            children = null;
            points = null;
        }

        //return copy of child node array - not sure if copy function is correct

        public AgentOctree[] getChildren()
        {
            if (children != null)
            {
                AgentOctree[] clones = new AgentOctree[8];
                Array.Copy(children, 0, clones, 0, 8);
                return clones;

            }
            return null;
        }


        //return depth

        public int getDepth() { return depth; }


        //find leaf node which spatially relates to the given point. p = point to check.
        //return leaf node or null if pt outside tree dimensions
        //something odd with brackets here

        public AgentOctree getLeafForPoint(Agent p)
        {
            //if not a leaf node

            if (containsPoint(p))
            {
                if (numChildren > 0)
                {
                    int octant = getOctantID(Vector3d.Subtract(p.position, offset));
                    if (children[octant] != null)
                    {
                        return children[octant].getLeafForPoint(p);
                    }
                    else if (points != null)
                    {
                        return this;
                    }
                }
            }
            return null;
        }

        //Return the min size of nodes (in world units) this value acts as
        // tree recursion limit since nodes smaller than this size are not subdivided further
        // leaf nodes are always smaller?  or equal to this size

        //return min size of tree nodes

        public double getMinNodeSize()
        {
            return minNodeSize;
        }
        public double getNodeSize()
        {
            return size;
        }

        //return the number of child nodes (max 8)

        public int getNumChildren() { return numChildren; }


        //compute the local child octant/cube index for the given ppoint
        //p local is point in the node local coord system
        //return the octant index

        public int getOctantID(Vector3d plocal)
        {

            return (plocal.X >= halfSize ? 1 : 0) + (plocal.Y >= halfSize ? 2 : 0) + (plocal.Z >= halfSize ? 4 : 0);

        }

        public Vector3d getOffset() { return offset; }

        public AgentOctree getParent() { return parent; }

        //return the points

        public List<Agent> getPoints()
        {

            List<Agent> results = null;
            if (points != null) { results = new List<Agent>(points); }
            else if (numChildren > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i] != null)
                    {
                        List<Agent> childPoints = children[i].getPoints();
                        if (childPoints != null)
                        {
                            if (results == null) { results = new List<Agent>(); }
                            results.AddRange(childPoints);
                        }
                    }

                }

            }
            return results;
        }



        //get all points within AABB
        /*
        public List<Vector3d> getPointsWithinBox(AABB b){
        // List<Vector3d> results = null;
        // if (this.intersectsBox()
        }
        */

        //Get within sphere

        public List<Agent> getPointsWithinSphere(Agent sphereCentre, float sphereRadius)
        {

            List<Agent> results = new List<Agent>();
           // List<Agent> closestresults = new List<Agent>();
            //this is silly but lets test it
           /* int closestIndex = -1;
            int secondClosestIndex = -1;
            int thirdClosestIndex = -1;
            int fourthClosestIndex = -1;

            Double closestDistance = 2000;
            Double secondClosestDistance = 2000;
            Double thirdClosestDistance = 2000;
            Double fourthClosestDistance = 2000;
            */

            //if the sphere intersects with the bounding box, continue
            if (this.intersectsSphere(sphereCentre.position, sphereRadius))
            {
                //if the list of points is full,
                if (points != null)
                {
                    foreach (var q in points)
                    {

                       
                        //if sphere s contactsPoint q - code from sphere class
                        //Vector3d test = new Vector3d(0, 0, 0);
                        //make sure not testing actual agents pos

                        if (q.position != sphereCentre.position) { 

                        if (containsPoint(q, sphereCentre.position, sphereRadius))
                        {
                            

                            if (results == null)
                            {
                                results = new List<Agent>();
                            }
                            results.Add(q);
                        }
                    }
                }
                }

                else if (numChildren > 0)
                {

                    for (int i = 0; i < 8; i++)
                    {

                        if (children[i] != null)
                        {
                            // List<Vector3d> points TEST
                            //  points = children[i].getPointsWithinSphere(sphereCentre, sphereRadius); remove poitns2

                            /*List<Vector3d>*/
                            points = children[i].getPointsWithinSphere(sphereCentre, sphereRadius);


                            if (points != null)
                            {
                                if (results == null)
                                {

                                    results = new List<Agent>();

                                }
                                results.AddRange(points);
                            }
                        }
                    }
                }
            }

            if (results.Count() > 0) { 
           //test agent distances
           for (int i = 0; i < results.Count()-1; i++)
            {

             //   Agent temp = results.ElementAt(i);

                    //neighbour minus searching agent
                  //  Vector3d vecBetweenAgents = temp.position - sphereCentre.position;
                  //  double dist = vecBetweenAgents.Length;
                   // if (dist < 0) { dist = (dist * -1); }
                   /*
                    if (dist < closestDistance) {

                        fourthClosestIndex = thirdClosestIndex;
                        thirdClosestIndex = secondClosestIndex;
                        secondClosestIndex = closestIndex;
                        closestIndex = i;

                        fourthClosestDistance = thirdClosestDistance;
                        thirdClosestDistance = secondClosestDistance;
                        secondClosestDistance = closestDistance;                  
                        closestDistance = dist; 
                    }
                    */


                }
            }

            /*
            int resultCount = results.Count();

            if (resultCount >= 4) { 
            closestresults.Add(results.ElementAt(closestIndex));
            closestresults.Add(results.ElementAt(secondClosestIndex));
            closestresults.Add(results.ElementAt(thirdClosestIndex));
            closestresults.Add(results.ElementAt(fourthClosestIndex));
            }

            if (resultCount == 3)
            {
                closestresults.Add(results.ElementAt(closestIndex));
                closestresults.Add(results.ElementAt(secondClosestIndex));
                closestresults.Add(results.ElementAt(thirdClosestIndex));
            }

            if (resultCount == 2)
            {
                closestresults.Add(results.ElementAt(closestIndex));
                closestresults.Add(results.ElementAt(secondClosestIndex));
            }

            /*
            if (resultCount == 1)
            {
                closestresults.Add(results.ElementAt(closestIndex));
                
            }
            */
            
           // return closestresults;
            return results;
        }


        public bool containsPoint(Agent testPoint, Vector3d sphereCentre, float sphereRadius)
        {

            // Vector3d sub = new Vector3d();

            //  sub = Vector3d.Subtract(sphereCentre, testPoint);
            //
            //  double d = sub.SquareLength;
            if (testPoint.position != sphereCentre) { 
            double d = Vector3d.Subtract(sphereCentre, testPoint.position).SquareLength;
            return (d <= sphereRadius * sphereRadius);
            }else
            {
                return false;
            }
        }



        public double getSize() { return size; }



        public void reduceBranch()
        {
            if (points != null && points.Count() == 0)
            {
                points = null;
            }
            if (numChildren > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i] != null && children[i].points == null)
                    {
                        children[i] = null;
                    }
                }
            }
            if (parent != null) { parent.reduceBranch(); }
        }


        // public vboolean remove

        //public void removeAll

        public void setMinNodeSize(double minNodeSize) { this.minNodeSize = minNodeSize * 0.5; }










    }
}
