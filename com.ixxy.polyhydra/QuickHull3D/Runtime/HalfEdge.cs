﻿/*
 * Copyright John E. Lloyd, 2004. All rights reserved. Permission to use,
 * copy, modify and redistribute is granted, provided that this copyright
 * notice is retained and the author is given credit whenever appropriate.
 *
 * This  software is distributed "as is", without any warranty, including 
 * any implied warranty of merchantability or fitness for a particular
 * use. The author assumes no responsibility for, and shall not be liable
 * for, any special, indirect, or consequential damages, or any damages
 * whatsoever, arising out of or in connection with the use of this
 * software.
 */


namespace QuickHull3D
{
    /// <summary>
    /// Represents the half-edges that surround each
    /// face in a counter-clockwise direction.
    /// </summary>
    /// 
    /// author John E. Lloyd, Fall 2004
    public class HalfEdge
    {

        /// <summary>
        /// Triangular face associated with this half-edge.
        /// (located to the left of this half-edge).
        /// </summary>

        public Face Face { get; set; }

        
        /// <summary>
        /// Next half-edge in the triangle.
        /// (counter-clockwise) to this one within the triangle
        /// </summary>

        public HalfEdge Next { get; set; }

        /// <summary>
        /// Previous half-edge in the triangle.
        /// Adjacent (clockwise) to this one within the triangle.
        /// </summary>
        public HalfEdge Prev { get; set; }

        /// <summary>
        /// Half-edge associated with the opposite triangle
        /// adjacent to this edge.
        /// </summary>
        public HalfEdge Opposite { get; set; }

        /// <summary>
        /// Constructs a HalfEdge with head vertex <code>v</code> and
        /// left-hand triangular face <code>f</code>.
        /// </summary>
        /// <param name="v">head vertex</param>
        /// <param name="f">left-hand triangular face</param>
        public HalfEdge(Vertex v, Face f)
        {
            Head = v;
            Face = f;
        }

        public HalfEdge()
        {
        }

        /// <summary>
        /// Sets the half-edge opposite to this half-edge.
        /// </summary>
        /// <param name="edge">opposite half-edge</param>
        public void SetMutualOpposites(HalfEdge edge)
        {
            Opposite = edge;
            edge.Opposite = this;
        }

        /// <summary>
        /// The head vertex associated with this half-edge.
        /// </summary>
        public Vertex Head { get; }

        /// <summary>
        /// The tail vertex associated with this half-edge.
        /// </summary>
        public Vertex Tail
            => Prev != null ? Prev.Head : null;


        /// <summary>
        /// The opposite triangular face associated with this
        /// half-edge.
        /// </summary>
        public Face OppositeFace
            => Opposite != null ? Opposite.Face : null;

        /// <summary>
        /// A string identifying this half-edge by the point
        /// index values of its tail and head vertices.
        /// </summary>
        public string VertexString
            => (Tail != null) ? $"{Tail.Index}-{Head.Index}" : $"?-{Head.Index}";

        /// <summary>
        /// The length of this half-edge.
        /// </summary>
        public float Length
            => (Tail != null) ? Head.Point.Distance(Tail.Point) : -1;

        /// <summary>
        /// The length squared of this half-edge.
        /// </summary>
        public float LengthSquared
            => (Tail != null) ? Head.Point.DistanceSquared(Tail.Point) : -1;
     

        // 	/**
        // 	 * Computes nrml . (del0 X del1), where del0 and del1
        // 	 * are the direction vectors along this halfEdge, and the
        // 	 * halfEdge he1.
        // 	 *
        // 	 * A product > 0 indicates a left turn WRT the normal
        // 	 */
        // 	public float turnProduct (HalfEdge he1, Vector3d nrml)
        // 	 { 
        // 	   Point3d pnt0 = tail().pnt;
        // 	   Point3d pnt1 = head().pnt;
        // 	   Point3d pnt2 = he1.head().pnt;

        // 	   float del0x = pnt1.x - pnt0.x;
        // 	   float del0y = pnt1.y - pnt0.y;
        // 	   float del0z = pnt1.z - pnt0.z;

        // 	   float del1x = pnt2.x - pnt1.x;
        // 	   float del1y = pnt2.y - pnt1.y;
        // 	   float del1z = pnt2.z - pnt1.z;

        // 	   return (nrml.x*(del0y*del1z - del0z*del1y) + 
        // 		   nrml.y*(del0z*del1x - del0x*del1z) + 
        // 		   nrml.z*(del0x*del1y - del0y*del1x));
        // 	 }
    }


}
