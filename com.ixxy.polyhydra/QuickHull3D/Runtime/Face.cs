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

using System;

namespace QuickHull3D
{
    /// <summary>
    /// Basic triangular face used to form the hull.
    /// </summary>
    /// 
    /// <p>The information stored for each face consists of a planar
    /// normal, a planar offset, and a doubly-linked list of three
    /// <see cref="HalfEdge">HalfEdges</see> which surround the face in a
    /// counter-clockwise direction.
    /// 
    /// author John E. Lloyd, Fall 2004
    public class Face
    {
        public HalfEdge He0 { get; private set; }

        /// <summary>
        /// The normal of the plane associated with this face.
        /// </summary>
        public Vector3d Normal { get; private set; }
        public double Area { get; private set; }
        public Point3d Centroid { get; private set; }
        public Face Next { get; set; }
        public int Mark { get; set; } = VISIBLE;
        public Vertex Outside { get; set; }
        public int VertexCount { get; private set; }


        private double planeOffset;
        public const int VISIBLE = 1;
        public const int NON_CONVEX = 2;
        public const int DELETED = 3;

        public void ComputeCentroid(Point3d centroid)
        {
            centroid.SetAll(0.0);
            HalfEdge he = He0;
            do
            {
                centroid.Add(he.Head.Point);
                he = he.Next;
            }
            while (he != He0);
            centroid.Scale(1.0 / VertexCount);
        }

        public void ComputeNormal(Vector3d normal, double minArea)
        {
            ComputeNormal(normal);

            if (Area < minArea)
            {
                // make the normal more robust by removing
                // components parallel to the longest edge

                HalfEdge hedgeMax = null;
                double lenSqrMax = 0;
                HalfEdge hedge = He0;
                do
                {
                    double lenSqr = hedge.LengthSquared;
                    if (lenSqr > lenSqrMax)
                    {
                        hedgeMax = hedge;
                        lenSqrMax = lenSqr;
                    }
                    hedge = hedge.Next;
                }
                while (hedge != He0);

                Point3d p2 = hedgeMax.Head.Point;
                Point3d p1 = hedgeMax.Tail.Point;
                double lenMax = Math.Sqrt(lenSqrMax);
                double ux = (p2.x - p1.x) / lenMax;
                double uy = (p2.y - p1.y) / lenMax;
                double uz = (p2.z - p1.z) / lenMax;
                double dot = normal.x * ux + normal.y * uy + normal.z * uz;
                normal.x -= dot * ux;
                normal.y -= dot * uy;
                normal.z -= dot * uz;

                normal.Normalize();
            }
        }

        public void ComputeNormal(Vector3d normal)
        {
            HalfEdge he1 = He0.Next;
            HalfEdge he2 = he1.Next;

            Point3d p0 = He0.Head.Point;
            Point3d p2 = he1.Head.Point;

            double d2x = p2.x - p0.x;
            double d2y = p2.y - p0.y;
            double d2z = p2.z - p0.z;

            normal.SetAll(0.0);

            VertexCount = 2;

            while (he2 != He0)
            {
                double d1x = d2x;
                double d1y = d2y;
                double d1z = d2z;

                p2 = he2.Head.Point;
                d2x = p2.x - p0.x;
                d2y = p2.y - p0.y;
                d2z = p2.z - p0.z;

                normal.x += d1y * d2z - d1z * d2y;
                normal.y += d1z * d2x - d1x * d2z;
                normal.z += d1x * d2y - d1y * d2x;

                he1 = he2;
                he2 = he2.Next;
                VertexCount++;
            }
            Area = normal.Norm;
            normal.Scale(1 / Area);
        }

        private void ComputeNormalAndCentroid()
        {
            ComputeNormal(Normal);
            ComputeCentroid(Centroid);
            planeOffset = Normal.Dot(Centroid);
            int numv = 0;
            HalfEdge he = He0;
            do
            {
                numv++;
                he = he.Next;
            }
            while (he != He0);
            if (numv != VertexCount)
            {
                throw new InternalErrorException($"face {VertexString} numVerts={VertexCount} should be {numv}");
            }
        }

        private void ComputeNormalAndCentroid(double minArea)
        {
            ComputeNormal(Normal, minArea);
            ComputeCentroid(Centroid);
            planeOffset = Normal.Dot(Centroid);
        }


        /// <summary>
        /// Constructs a triangule Face from vertices v0, v1, and v2.
        /// </summary>
        /// 
        /// <param name="v0">first vertex</param>
        /// <param name="v1">second vertex</param>
        /// <param name="v2">third vertex</param>
        public static Face CreateTriangle(Vertex v0, Vertex v1, Vertex v2)
        {
            return CreateTriangle(v0, v1, v2, 0);
        }

        public static Face CreateTriangle(Vertex v0, Vertex v1, Vertex v2, double minArea)
        {
            Face face = new Face();
            HalfEdge he0 = new HalfEdge(v0, face);
            HalfEdge he1 = new HalfEdge(v1, face);
            HalfEdge he2 = new HalfEdge(v2, face);

            he0.Prev = he2;
            he0.Next = he1;
            he1.Prev = he0;
            he1.Next = he2;
            he2.Prev = he1;
            he2.Next = he0;

            face.He0 = he0;

            // compute the normal and offset
            face.ComputeNormalAndCentroid(minArea);
            return face;
        }

        public static Face Create(Vertex[] vtxArray, int[] indices)
        {
            Face face = new Face();
            HalfEdge hePrev = null;
            for (int i = 0; i < indices.Length; i++)
            {
                HalfEdge he = new HalfEdge(vtxArray[indices[i]], face);
                if (hePrev != null)
                {
                    he.Prev = hePrev;
                    hePrev.Next = he;
                }
                else
                {
                    face.He0 = he;
                }
                hePrev = he;
            }
            face.He0.Prev = hePrev;
            hePrev.Next = face.He0;

            // compute the normal and offset
            face.ComputeNormalAndCentroid();
            return face;
        }

        public Face()
        {
            Normal = new Vector3d();
            Centroid = new Point3d();
            Mark = VISIBLE;
        }

        /// <summary>
        /// Gets the i-th half-edge associated with the face.
        /// </summary>
        /// 
        /// <param name="i">the half-edge index, in the range 0-2.</param>
        /// <returns>the half-edge</returns>
        public HalfEdge GetEdge(int i)
        {
            HalfEdge he = He0;
            while (i > 0)
            {
                he = he.Next;
                i--;
            }
            while (i < 0)
            {
                he = he.Prev;
                i++;
            }
            return he;
        }

        public HalfEdge FirstEdge => He0;

        /// <summary>
        /// Finds the half-edge within this face which has
        /// tail <code>vt</code> and head <code>vh</code>.
        /// </summary>
        /// <param name="vh">tail point</param>
        /// <param name="vt">head point</param> 
        /// <returns>the half-edge, or null if none is found.</returns> 
        public HalfEdge FindEdge(Vertex vt, Vertex vh)
        {
            HalfEdge he = He0;
            do
            {
                if (he.Head == vh && he.Tail == vt)
                {
                    return he;
                }
                he = he.Next;
            }
            while (he != He0);
            return null;
        }

        /// <summary>
        /// Computes the distance from a point p to the plane of
        /// this face.
        /// </summary>
        /// 
        /// <param name="p">the point</param>
        /// <returns>distance from the point to the plane</returns>
        public double DistanceToPlane(Point3d p)
        {
            return Normal.x * p.x + Normal.y * p.y + Normal.z * p.z - planeOffset;
        }

        public string VertexString
        {
            get
            { 
                string s = null;
                HalfEdge he = He0;
                do
                {
                    if (s == null)
                    {
                        s = "" + he.Head.Index;
                    }
                    else
                    {
                        s += " " + he.Head.Index;
                    }
                    he = he.Next;
                }
                while (he != He0);
                return s;
            }
        }

        public void GetVertexIndices(int[] idxs)
        {
            HalfEdge he = He0;
            int i = 0;
            do
            {
                idxs[i++] = he.Head.Index;
                he = he.Next;
            }
            while (he != He0);
        }

        private Face ConnectHalfEdges(
           HalfEdge hedgePrev, HalfEdge hedge)
        {
            Face discardedFace = null;

            if (hedgePrev.OppositeFace == hedge.OppositeFace)
            { // then there is a redundant edge that we can get rid off

                Face oppFace = hedge.OppositeFace;
                HalfEdge hedgeOpp;

                if (hedgePrev == He0)
                {
                    He0 = hedge;
                }
                if (oppFace.VertexCount == 3)
                { // then we can get rid of the opposite face altogether
                    hedgeOpp = hedge.Opposite.Prev.Opposite;

                    oppFace.Mark = DELETED;
                    discardedFace = oppFace;
                }
                else
                {
                    hedgeOpp = hedge.Opposite.Next;

                    if (oppFace.He0 == hedgeOpp.Prev)
                    {
                        oppFace.He0 = hedgeOpp;
                    }
                    hedgeOpp.Prev = hedgeOpp.Prev.Prev;
                    hedgeOpp.Prev.Next = hedgeOpp;
                }
                hedge.Prev = hedgePrev.Prev;
                hedge.Prev.Next = hedge;

                hedge.Opposite = hedgeOpp;
                hedgeOpp.Opposite = hedge;

                // oppFace was modified, so need to recompute
                oppFace.ComputeNormalAndCentroid();
            }
            else
            {
                hedgePrev.Next = hedge;
                hedge.Prev = hedgePrev;
            }
            return discardedFace;
        }

        public void CheckConsistency()
        {
            // do a sanity check on the face
            HalfEdge hedge = He0;
            double maxd = 0;
            int numv = 0;

            if (VertexCount < 3)
            {
                throw new InternalErrorException($"degenerate face: {VertexString}");
            }
            do
            {
                HalfEdge hedgeOpp = hedge.Opposite;
                if (hedgeOpp == null)
                {
                    throw new InternalErrorException($"face {VertexString}: unreflected half edge {hedge.VertexString}");
                }
                else if (hedgeOpp.Opposite != hedge)
                {
                    throw new InternalErrorException($"face {VertexString}: opposite half edge {hedgeOpp.VertexString} has opposite {hedgeOpp.Opposite.VertexString}");
                }
                if (hedgeOpp.Head != hedge.Tail ||
                hedge.Head != hedgeOpp.Tail)
                {
                    throw new InternalErrorException($"face {VertexString}: half edge {hedge.VertexString} reflected by {hedgeOpp.VertexString}");
                }
                Face oppFace = hedgeOpp.Face;
                if (oppFace == null)
                {
                    throw new InternalErrorException($"face {VertexString}: no face on half edge {hedgeOpp.VertexString}");
                }
                else if (oppFace.Mark == DELETED)
                {
                    throw new InternalErrorException($"face {VertexString}: opposite face {oppFace.VertexString} not on hull");
                }
                double d = Math.Abs(DistanceToPlane(hedge.Head.Point));
                if (d > maxd)
                {
                    maxd = d;
                }
                numv++;
                hedge = hedge.Next;
            }
            while (hedge != He0);

            if (numv != VertexCount)
            {
                throw new InternalErrorException($"face {VertexString} numVerts={VertexCount} should be {numv}");
            }

        }

        public int MergeAdjacentFace(HalfEdge hedgeAdj, Face[] discarded)
        {
            Face oppFace = hedgeAdj.OppositeFace;
            int numDiscarded = 0;

            discarded[numDiscarded++] = oppFace;
            oppFace.Mark = DELETED;

            HalfEdge hedgeOpp = hedgeAdj.Opposite;

            HalfEdge hedgeAdjPrev = hedgeAdj.Prev;
            HalfEdge hedgeAdjNext = hedgeAdj.Next;
            HalfEdge hedgeOppPrev = hedgeOpp.Prev;
            HalfEdge hedgeOppNext = hedgeOpp.Next;

            while (hedgeAdjPrev.OppositeFace == oppFace)
            {
                hedgeAdjPrev = hedgeAdjPrev.Prev;
                hedgeOppNext = hedgeOppNext.Next;
            }

            while (hedgeAdjNext.OppositeFace == oppFace)
            {
                hedgeOppPrev = hedgeOppPrev.Prev;
                hedgeAdjNext = hedgeAdjNext.Next;
            }

            HalfEdge hedge;

            for (hedge = hedgeOppNext; hedge != hedgeOppPrev.Next; hedge = hedge.Next)
            {
                hedge.Face = this;
            }

            if (hedgeAdj == He0)
            {
                He0 = hedgeAdjNext;
            }

            // handle the half edges at the head
            Face discardedFace;

            discardedFace = ConnectHalfEdges(hedgeOppPrev, hedgeAdjNext);
            if (discardedFace != null)
            {
                discarded[numDiscarded++] = discardedFace;
            }

            // handle the half edges at the tail
            discardedFace = ConnectHalfEdges(hedgeAdjPrev, hedgeOppNext);
            if (discardedFace != null)
            {
                discarded[numDiscarded++] = discardedFace;
            }

            ComputeNormalAndCentroid();
            CheckConsistency();

            return numDiscarded;
        }

        private double AreaSquared(HalfEdge hedge0, HalfEdge hedge1)
        {
            // return the squared area of the triangle defined
            // by the half edge hedge0 and the point at the
            // head of hedge1.

            Point3d p0 = hedge0.Tail.Point;
            Point3d p1 = hedge0.Head.Point;
            Point3d p2 = hedge1.Head.Point;

            double dx1 = p1.x - p0.x;
            double dy1 = p1.y - p0.y;
            double dz1 = p1.z - p0.z;

            double dx2 = p2.x - p0.x;
            double dy2 = p2.y - p0.y;
            double dz2 = p2.z - p0.z;

            double x = dy1 * dz2 - dz1 * dy2;
            double y = dz1 * dx2 - dx1 * dz2;
            double z = dx1 * dy2 - dy1 * dx2;

            return x * x + y * y + z * z;
        }

        public void Triangulate(FaceList newFaces, double minArea)
        {
            HalfEdge hedge;

            if (VertexCount < 4)
            {
                return;
            }

            Vertex v0 = He0.Head;

            hedge = He0.Next;
            HalfEdge oppPrev = hedge.Opposite;
            Face face0 = null;

            for (hedge = hedge.Next; hedge != He0.Prev; hedge = hedge.Next)
            {
                Face face =
               CreateTriangle(v0, hedge.Prev.Head, hedge.Head, minArea);
                face.He0.Next.SetMutualOpposites(oppPrev);
                face.He0.Prev.SetMutualOpposites(hedge.Opposite);
                oppPrev = face.He0;
                newFaces.Add(face);
                if (face0 == null)
                {
                    face0 = face;
                }
            }
            hedge = new HalfEdge(He0.Prev.Prev.Head, this);
            hedge.SetMutualOpposites(oppPrev);

            hedge.Prev = He0;
            hedge.Prev.Next = hedge;

            hedge.Next = He0.Prev;
            hedge.Next.Prev = hedge;

            ComputeNormalAndCentroid(minArea);
            CheckConsistency();

            for (Face face = face0; face != null; face = face.Next)
            {
                face.CheckConsistency();
            }

        }
    }



}
