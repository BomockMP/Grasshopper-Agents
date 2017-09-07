using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace AgentSystem
{
  public class AABB
    {
        //axis aligned bounding box
        //create a new instance from two vectors specifying opposite corners of the box
        //parameter min = first corner point
        //para max = second corner point
        //return a new AABB with centre at the half point between the 2 input vectors







        //variables
        public Vector3d centrePoint = new Vector3d();
        public float extentsFloat;
        public Vector3d extent = new Vector3d();
        public Vector3d min;
        public Vector3d max;
        public Vector3d a;
        public Vector3d b;




        //CONSTRUCTOR THAT TAKES a Vector3d midpoint and a vector3d extents (this is what the octree will give)
        //how can i get around this? identify the two constructors?
        public AABB(Vector3d centrePos, Vector3d extentFromCentre)
        {

            centrePoint = centrePos;
            setExtent(extentFromCentre);

        }



        //INITIALISING FUNCTION

        public AABB fromMinMax(Vector3d _min, Vector3d _max)
        {

            Vector3d min = _min;
            Vector3d max = _max;


            //based on the two imput vectors, find the minimum vector and the max vector.
            //calculated by creating vectors based on the smallest componenets of both and then the greatest components of both.
            //this will ensure that the bounding box contains these two passed in vectors.

            a = findMin(min, max);
            b = findMax(min, max);

            centrePoint = interpolateVectors(a, b, 0.5);
            setExtent(Vector3d.Multiply(Vector3d.Subtract(b, a), 0.5));

            AABB fromPts = new AABB(centrePoint, extent);
            return fromPts;


        }


        //vector min/max function, interpolte functions


        //find the min vector and the max vector, by comparing each vectors x, y, z values and finding the 'least' combination and the 'most'
        public static Vector3d findMin(Vector3d a, Vector3d b)
        {

            return new Vector3d(calcMin(a.X, b.X), calcMin(a.Y, b.Y), calcMin(a.Z, b.Z));
        }

        public static Vector3d findMax(Vector3d a, Vector3d b)
        {

            return new Vector3d(calcMax(a.X, b.X), calcMax(a.Y, b.Y), calcMax(a.Z, b.Z));
        }

        //is a < b? then return a. otherwise return b
        public static double calcMin(double a, double b)
        {
            return a < b ? a : b;
        }

        //is a > b? then return a. otherwise, return b.

        public static double calcMax(double a, double b)
        {
            return a > b ? a : b;
        }


        public static Vector3d interpolateVectors(Vector3d v1, Vector3d v2, double f)
        {
            //interpolate vectors and return new vector
            return new Vector3d(v1.X + (v2.X - v1.X) * f, v1.Y + (v2.Y - v1.Y) * f, v1.Z + (v2.Z - v1.Z) * f);

        }



        public AABB setExtent(Vector3d extents)
        {
            //sets the extents of the box, is called in the constructor
            this.extent = extents;
            return updateBounds();

        }

        public AABB updateBounds()
        {
            //is called once the extents are set (ie, called in the constructor also)
            if (extent != null)
            {

                //construct a new vector based on the smallest components of both vectors

                this.min = Vector3d.Subtract(centrePoint, extent);
                this.max = Vector3d.Add(centrePoint, extent);

            }

            return this;

        }

        //should make these more secure so that you cant change them from outside.
        public Vector3d getMin()
        {
            return min;
            // return min;
        }

        public Vector3d getMax()
        {
            return max;
            //  return max;
        }

        //intersect box, not complete



        //intersect sphere,
        //i dont see how this is ever false if inside the boudns of the box. tests whether the sphere
        //interscts with the box.


        public bool intersectsSphere(Vector3d c, double r)
        {
            double s = 0;
            double d = 0;
            //find the sq of the distance from the sphere to the vector
            if (c.X < min.X)
            {
                s = c.X - min.X;
                d = s * s;


            }
            else if (c.X > max.X)
            {
                s = c.X - max.X;
                d += s * s;
            }
            if (c.Y < min.Y)
            {
                s = c.Y - min.Y;
                d += s * s;

            }
            else if (c.Y > max.Y)
            {
                s = c.Y - max.Y;
                d += s * s;
            }

            if (c.Z < min.Z) { s = c.Z - min.Z; d += s * s; } else if (c.Z > max.Z) { s = c.Z - max.Z; d += s * s; }

            return d <= r * r;
        }

        //Render Box

        public Box renderBBox()
        {
            Point3d corner1 = new Point3d(min.X, min.Y, min.Z);
            Point3d corner2 = new Point3d(max.X, max.Y, max.Z);
            BoundingBox bbox = new BoundingBox(corner1, corner2);
            Box renderBox = new Box(bbox);
            return renderBox;

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
    }
}
