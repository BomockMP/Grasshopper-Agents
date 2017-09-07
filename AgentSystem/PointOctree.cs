using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;



namespace AgentSystem
{
    public class PointOctree : AABB
    {
        //Variables OCTREE//
        //changed all to public
        public double minNodeSize = 4; //4
        public PointOctree parent;
        public PointOctree[] children;
        public byte numChildren;
        public List<Vector3d> points;

        public double size;
        public double halfSize;
        public Vector3d offset;
        private int depth = 0;
        //private bool isAutoReducing = false;




        //constructor // :base is the same as super to pass down to AABB constructor

        public PointOctree(PointOctree p, Vector3d o, double halfSize) :

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



        public bool addAll(List<Vector3d> points)
        {

            bool addedAll = true;

            foreach (var p in points)
            {
                addedAll &= addPoint(p);
            }
            return addedAll;
        }


        public bool addPoint(Vector3d p)
        {
            //check if pt is inside cube - write code for contains point

            if (containsPoint(p))
            {

                if (halfSize <= minNodeSize)
                {

                    if (points == null)
                    {

                        points = new List<Vector3d>();

                    }

                    points.Add(p);

                    return true;

                }
                else
                {

                    Vector3d plocal = Vector3d.Subtract(p, offset);

                    if (children == null)
                    {
                        children = new PointOctree[8];

                    }


                    int octant = getOctantID(plocal);
                    if (children[octant] == null)
                    {
                        Vector3d off = Vector3d.Add(offset, new Vector3d(
(octant & 1) != 0 ? halfSize : 0,
(octant & 2) != 0 ? halfSize : 0,
(octant & 4) != 0 ? halfSize : 0));

                        children[octant] = new PointOctree(this, off, halfSize * 0.5);

                        numChildren++;

                    }
                    return children[octant].addPoint(p);

                }


            }
            return false;

        }




        public bool containsPoint(Vector3d p)
        {
            //    return //check whether in AABB..NOT SURE WHERE THIS CHECK SHOULD BE. IDEALLY IT IS IN AGENT TOO, BUT WE ARE WORKING WITH GENERIC VECTOR3DS HERE. SO
            // WILL DOUBLE UP FOR NOW.

            //return p.isInAABB(this);

            //check whether vector3d p is in AABB ('this', as the class is an extension of AABB. find bounds of box
            Vector3d min = this.getMin();
            Vector3d max = this.getMax();
            if (p.X < min.X || p.X > max.X) { return false; }
            if (p.Y < min.Y || p.Y > max.Y) { return false; }
            if (p.Z < min.Z || p.Z > max.Z) { return false; }

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

        public PointOctree[] getChildren()
        {
            if (children != null)
            {
                PointOctree[] clones = new PointOctree[8];
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

        public PointOctree getLeafForPoint(Vector3d p)
        {
            //if not a leaf node

            if (containsPoint(p))
            {
                if (numChildren > 0)
                {
                    int octant = getOctantID(Vector3d.Subtract(p, offset));
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

        public PointOctree getParent() { return parent; }

        //return the points

        public List<Vector3d> getPoints()
        {

            List<Vector3d> results = null;
            if (points != null) { results = new List<Vector3d>(points); }
            else if (numChildren > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (children[i] != null)
                    {
                        List<Vector3d> childPoints = children[i].getPoints();
                        if (childPoints != null)
                        {
                            if (results == null) { results = new List<Vector3d>(); }
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

        public List<Vector3d> getPointsWithinSphere(Vector3d sphereCentre, float sphereRadius)
        {

            List<Vector3d> results = new List<Vector3d>();


            //if the sphere intersects with the bounding box, continue
            if (this.intersectsSphere(sphereCentre, sphereRadius))
            {
                //if the list of points is full,
                if (points != null)
                {
                    foreach (var q in points)
                    {
                        //if sphere s contactsPoint q - code from sphere class
                        //Vector3d test = new Vector3d(0, 0, 0);

                        if (containsPoint(q, sphereCentre, sphereRadius))
                        {
                            if (results == null)
                            {
                                results = new List<Vector3d>();
                            }
                            results.Add(q);
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

                                    results = new List<Vector3d>();

                                }
                                results.AddRange(points);
                            }
                        }
                    }
                }
            }
            return results;
        }


        public bool containsPoint(Vector3d testPoint, Vector3d sphereCentre, float sphereRadius)
        {

            // Vector3d sub = new Vector3d();

            //  sub = Vector3d.Subtract(sphereCentre, testPoint);
            //
            //  double d = sub.SquareLength;

            double d = Vector3d.Subtract(sphereCentre, testPoint).SquareLength;
            return (d <= sphereRadius * sphereRadius);
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
