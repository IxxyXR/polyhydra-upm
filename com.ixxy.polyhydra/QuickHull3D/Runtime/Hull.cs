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
using System.Collections.Generic;
using System.IO;

namespace QuickHull3D
{

    /// <summary>
    /// Computes the convex hull of a set of three dimensional points.
    /// 
    ///
    /// <p>The algorithm is a three dimensional implementation of Quickhull, as
    /// described in Barber, Dobkin, and Huhdanpaa,
    /// <a href=http://citeseer.ist.psu.edu/barber96quickhull.html> ``The Quickhull
    /// Algorithm for Convex Hulls''</a> (ACM Transactions on Mathematical Software,
    /// Vol. 22, No. 4, December 1996), and has a complexity of O(n log(n)) with
    /// respect to the number of points. A well-known C implementation of Quickhull
    /// that works for arbitrary dimensions is provided by
    /// <a href=http://www.qhull.org>qhull</a>.
    ///
    /// <p>A hull is constructed by providing a set of points
    /// to either a constructor or a
    /// <see cref="Build(Point3d[])"/> method. After
    /// the hull is built, its vertices and faces can be retrieved
    /// using <see cref="GetVertices()"/> and <see cref="GetFaces()"/>.
    /// A typical usage might look like this:
    /// 
    /// <code>
    ///    // x y z coordinates of 6 points
    ///    Point3d[] points = new Point3d[] 
    ///    {
    ///       new Point3d (0.0,  0.0,  0.0),
    ///       new Point3d (1.0,  0.5,  0.0),
    ///       new Point3d (2.0,  0.0,  0.0),
    ///       new Point3d (0.5,  0.5,  0.5),
    ///       new Point3d (0.0,  0.0,  2.0),
    ///       new Point3d (0.1,  0.2,  0.3),
    ///       new Point3d (0.0,  2.0,  0.0),
    ///    };
    ///
    ///    QuickHull3D hull = new QuickHull3D();
    ///    hull.build(points);
    ///
    ///    Console.Out.WriteLine("Vertices:");
    ///    Point3d[] vertices = hull.getVertices();
    ///    for (int i = 0; i < vertices.Length; i++)
    ///    {
    ///       Point3d pnt = vertices[i];
    ///       Console.Out.WriteLine (pnt.x + " " + pnt.y + " " + pnt.z);
    ///    }
    ///
    ///    Console.Out.WriteLine ("Faces:");
    ///    int[][] faceIndices = hull.getFaces();
    ///    for (int i = 0; i < faceIndices.Length; i++)
    ///    {
    ///       for (int k = 0; k < faceIndices[i].Length; k++)
    ///       {
    ///          Console.Out.WriteLine(faceIndices[i][k] + " ");
    ///       }
    ///       Console.Out.WriteLine("");
    ///    }
    /// </code>
    /// 
    /// As a convenience, there are also <see cref="Build(double[])"/>
    /// and <see cref="GetVertices(double[])"/> methods which
    /// pass point information using an array of doubles.
    ///
    /// <h3><a name=distTol>Robustness</h3> Because this algorithm uses floating
    /// point arithmetic, it is potentially vulnerable to errors arising from
    /// numerical imprecision.  We address this problem in the same way as
    /// <a href=http://www.qhull.org>qhull</a>, by merging faces whose edges are not
    /// clearly convex. A face is convex if its edges are convex, and an edge is
    /// convex if the centroid of each adjacent plane is clearly <i>below</i> the
    /// plane of the other face. The centroid is considered below a plane if its
    /// distance to the plane is less than the negative of a
    /// <see cref="getDistanceTolerance()">distance tolerance</see>.
    /// This tolerance represents the smallest distance that can be reliably computed
    /// within the available numeric precision. It is normally computed automatically
    /// from the point data, although an application may
    /// <see cref="setExplicitDistanceTolerance(double)">set this tolerance explicitly</see>.
    ///
    /// <p>Numerical problems are more likely to arise in situations where data
    /// points lie on or within the faces or edges of the convex hull. We have
    /// tested QuickHull3D for such situations by computing the convex hull of a
    /// random point set, then adding additional randomly chosen points which lie
    /// very close to the hull vertices and edges, and computing the convex
    /// hull again. The hull is deemed correct if <see cref="check()"/> returns
    /// <code>true</code>.  These tests have been successful for a large number of
    /// trials and so we are confident that QuickHull3D is reasonably robust.
    ///
    /// <h3>Merged Faces</h3> The merging of faces means that the faces returned by
    /// QuickHull3D may be convex polygons instead of triangles. If triangles are
    /// desired, the application may <see cref="Triangulate()"/> the faces, but
    /// it should be noted that this may result in triangles which are very small or
    /// thin and hence difficult to perform reliable convexity tests on. In other
    /// words, triangulating a merged face is likely to restore the numerical
    /// problems which the merging process removed. Hence is it
    /// possible that, after triangulation, <see cref="check()"/> will fail (the same
    /// behavior is observed with triangulated output from <a
    /// href=http://www.qhull.org>qhull</a>).
    ///
    /// <h3>Degenerate Input</h3>It is assumed that the input points
    /// are non-degenerate in that they are not coincident, colinear, or
    /// colplanar, and thus the convex hull has a non-zero volume.
    /// If the input points are detected to be degenerate within
    /// the <see cref="getDistanceTolerance()">distance tolerance</see>, an
    /// <see cref="ArgumentException"/>will be thrown.
    /// </summary>
    /// 
    /// author: John E. Lloyd, Fall 2004
    /// 
    public class Hull
    {
        /// <summary>
        /// Specifies that (on output) vertex indices for a face should be
        /// listed in clockwise order.
        /// </summary>
        public const int CLOCKWISE = 0x1;

        /// <summary>
        /// Specifies that (on output) the vertex indices for a face should be
        /// numbered starting from 1.
        /// </summary>
        public const int INDEXED_FROM_ONE = 0x2;

        /// <summary>
        /// Specifies that (on output) the vertex indices for a face should be
        /// numbered starting from 0.
        /// </summary>
        public const int INDEXED_FROM_ZERO = 0x4;

        /// <summary>
        /// Specifies that (on output) the vertex indices for a face should be
        /// numbered with respect to the original input points.
        /// </summary>
        public const int POINT_RELATIVE = 0x8;

        /// <summary>
        /// Specifies that the distance tolerance should be
        /// computed automatically from the input point data.
        /// </summary>
        public const double AUTOMATIC_TOLERANCE = -1;

        protected int findIndex = -1;

        // estimated size of the point set
        protected double charLength;

        /// <summary>
        /// Enables/Disables the printing of debugging diagnostics.
        /// </summary>
        public bool Debug { get; set; } = false;

        protected Vertex[] pointBuffer = new Vertex[0];
        protected int[] vertexPointIndices = new int[0];
        private Face[] discardedFaces = new Face[3];

        private Vertex[] maxVtxs = new Vertex[3];
        private Vertex[] minVtxs = new Vertex[3];

        //protected Vector faces = new Vector(16);
        //protected Vector horizon = new Vector(16);

        protected List<Face> faces = new List<Face>(16);
        protected List<HalfEdge> horizon = new List<HalfEdge>(16);

        private FaceList newFaces = new FaceList();
        private VertexList unclaimed = new VertexList();
        private VertexList claimed = new VertexList();

        protected int numVertices;
        protected int numFaces;
        protected int numPoints;


        /// <summary>
        /// Precision of a double.
        /// </summary>
        private const double DOUBLE_PREC = 2.2204460492503131e-16;


        /// <summary>
        /// Returns the distance tolerance that was used for the most recently
        /// computed hull. The distance tolerance is used to determine when
        /// faces are unambiguously convex with respect to each other, and when
        /// points are unambiguously above or below a face plane, in the
        /// presence of <see cref="distTol">numerical imprecision</see>. Normally,
        /// this tolerance is computed automatically for each set of input
        /// points, but it can be set explicitly by the application.
        /// </summary>
        /// <seealso cref="ExplicitDistanceTolerance"/>
        public double DistanceTolerance { get; private set; }

        /// <summary>
        /// Sets an explicit distance tolerance for convexity tests.
        /// If <see cref="AUTOMATIC_TOLERANCE"/> is specified (the default),
        /// then the tolerance will be computed automatically from the point data.
        /// </summary>
        /// <seealso cref="DistanceTolerance"/>
        public double ExplicitDistanceTolerance { get; set; } = AUTOMATIC_TOLERANCE;


        private void AddPointToFace(Vertex vtx, Face face)
        {
            vtx.Face = face;

            if (face.Outside == null)
            {
                claimed.Add(vtx);
            }
            else
            {
                claimed.InsertBefore(vtx, face.Outside);
            }
            face.Outside = vtx;
        }

        private void RemovePointFromFace(Vertex vtx, Face face)
        {
            if (vtx == face.Outside)
            {
                if (vtx.Next != null && vtx.Next.Face == face)
                {
                    face.Outside = vtx.Next;
                }
                else
                {
                    face.Outside = null;
                }
            }
            claimed.Remove(vtx);
        }

        private Vertex RemoveAllPointsFromFace(Face face)
        {
            if (face.Outside != null)
            {
                Vertex end = face.Outside;
                while (end.Next != null && end.Next.Face == face)
                {
                    end = end.Next;
                }
                claimed.Remove(face.Outside, end);
                end.Next = null;
                return face.Outside;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates an empty convex hull object.
        /// </summary>
        public Hull()
        {
        }

        /// <summary>
        /// Creates a convex hull object and initializes it to the convex hull
        /// of a set of points whose coordinates are given by an
        /// array of doubles.
        /// </summary>
        /// <param name="coords">x, y, and z coordinates of each input
        /// point. The length of this array will be three times
        /// the the number of input points.</param>
        /// <exception cref="ArgumentException">the number of input points is less
        /// than four, or the points appear to be coincident, colinear, or
        /// coplanar.</exception>
        public Hull(double[] coords)
        {
            Build(coords, coords.Length / 3);
        }

        /// <summary>
        /// Creates a convex hull object and initializes it to the convex hull
        /// of a set of points.
        /// </summary>
        /// <param name="points">input points.</param>
        /// <exception cref="ArgumentException">the number of input points is less
        /// than four, or the points appear to be coincident, colinear, or
        /// coplanar.</exception>
        public Hull(Point3d[] points)
        {
            Build(points, points.Length);
        }

        private HalfEdge FindHalfEdge(Vertex tail, Vertex head)
        {
            // brute force ... OK, since setHull is not used much
            foreach (var face in faces)
            {
                HalfEdge he = face.FindEdge(tail, head);
                if (he != null)
                {
                    return he;
                }
            }
            return null;
        }

        protected void SetHull(double[] coords, int nump,
                   int[][] faceIndices, int numf)
        {
            InitBuffers(nump);
            SetPoints(coords, nump);
            ComputeMaxAndMin();
            for (int i = 0; i < numf; i++)
            {
                Face face = Face.Create(pointBuffer, faceIndices[i]);
                HalfEdge he = face.He0;
                do
                {
                    HalfEdge heOpp = FindHalfEdge(he.Head, he.Tail);
                    if (heOpp != null)
                    {
                        he.SetMutualOpposites(heOpp);
                    }
                    he = he.Next;
                }
                while (he != face.He0);
                faces.Add(face);
            }
        }

        //private void printQhullErrors(Process proc)
        //{
        //    bool wrote = false;
        //    InputStream es = proc.getErrorStream();
        //while (es.available() > 0)

        //    {
        //        Console.Out.Write(es.read());
        //        wrote = true;
        //    }
        //if (wrote)

        //    {
        //        Console.Out.WriteLine("");
        //    }
        //}

        //protected void setFromQhull(double[] coords, int nump, bool triangulate)
        //{
        //    String commandStr = "./qhull i";
        //    if (triangulate)
        //    {
        //        commandStr += " -Qt";
        //    }
        //    try
        //    {
        //        Process proc = Runtime.getRuntime().exec(commandStr);
        //        TextWriter ps = new TextWriter(proc.getOutputStream());
        //        StreamTokenizer stok =
        //       new StreamTokenizer(
        //          new InputStreamReader(proc.getInputStream()));

        //        ps.WriteLine("3 " + nump);
        //        for (int i = 0; i < nump; i++)
        //        {
        //            ps.WriteLine(
        //           coords[i * 3 + 0] + " " +
        //           coords[i * 3 + 1] + " " +
        //           coords[i * 3 + 2]);
        //        }
        //        ps.flush();
        //        ps.close();
        //        Vector indexList = new Vector(3);
        //        stok.eolIsSignificant(true);
        //        printQhullErrors(proc);

        //        do
        //        {
        //            stok.nextToken();
        //        }
        //        while (stok.sval == null ||
        //           !stok.sval.startsWith("MERGEexact"));
        //        for (int i = 0; i < 4; i++)
        //        {
        //            stok.nextToken();
        //        }
        //        if (stok.ttype != StreamTokenizer.TT_NUMBER)
        //        {
        //            Console.Out.WriteLine("Expecting number of faces");
        //            System.exit(1);
        //        }
        //        int numf = (int)stok.nval;
        //        stok.nextToken(); // clear EOL
        //        int[][] faceIndices = new int[numf][];
        //        for (int i = 0; i < numf; i++)
        //        {
        //            indexList.clear();
        //            while (stok.nextToken() != StreamTokenizer.TT_EOL)
        //            {
        //                if (stok.ttype != StreamTokenizer.TT_NUMBER)
        //                {
        //                    Console.Out.WriteLine("Expecting face index");
        //                    System.exit(1);
        //                }
        //                indexList.add(0, new Integer((int)stok.nval));
        //            }
        //            faceIndices[i] = new int[indexList.Count];
        //            int k = 0;
        //            for (Iterator it = indexList.iterator(); it.hasNext();)
        //            {
        //                faceIndices[i][k++] = ((Integer)it.next()).intValue();
        //            }
        //        }
        //        setHull(coords, nump, faceIndices, numf);
        //    }
        //    catch (Exception e)
        //    {
        //        e.printStackTrace();
        //        System.exit(1);
        //    }
        //}

        private void PrintPoints(TextWriter ps)
        {
            for (int i = 0; i < numPoints; i++)
            {
                Point3d pnt = pointBuffer[i].Point;
                ps.WriteLine($"{pnt.x}, {pnt.y}, {pnt.z},");
            }
        }

        /// <summary>
        /// Constructs the convex hull of a set of points whose
        /// coordinates are given by an array of doubles.
        /// </summary>
        /// <param name="coords">x, y, and z coordinates of each input
        /// point. The length of this array will be three times
        /// the number of input points.</param>
        /// <exception cref="ArgumentException">the number of input points is less
        /// than four, or the points appear to be coincident, colinear, or
        /// coplanar.</exception>
        public void Build(double[] coords)
        {
            Build(coords, coords.Length / 3);
        }

        /// <summary>
        /// Constructs the convex hull of a set of points whose
        /// coordinates are given by an array of doubles.
        /// </summary>
        /// <param name="coords">x, y, and z coordinates of each input</param>
        /// point. The length of this array must be at least three times
        /// <code>nump</code>.
        /// <param name="nump">number of input points</param>
        /// <exception cref="ArgumentException">the number of input points is less
        /// than four or greater than 1/3 the length of <code>coords</code>,
        /// or the points appear to be coincident, colinear, or
        /// coplanar.</exception>
        public void Build(double[] coords, int nump)
        {
            if (nump < 4)

            {
                throw new ArgumentException("Less than four input points specified");
            }
            if (coords.Length / 3 < nump)

            {
                throw new ArgumentException("Coordinate array too small for specified number of points");
            }
            InitBuffers(nump);
            SetPoints(coords, nump);
            BuildHull();
        }

        /// <summary>
        /// Constructs the convex hull of a set of points.
        /// </summary>
        /// <param name="points">input points</param>
        /// <exception cref="ArgumentException">the number of input points is less
        /// than four, or the points appear to be coincident, colinear, or
        /// coplanar.</exception>
        public void Build(Point3d[] points)
        {
            Build(points, points.Length);
        }

        /// <summary>
        /// Constructs the convex hull of a set of points.
        /// </summary>
        /// <param name="points">input points</param>
        /// <param name="nump">number of input points</param>
        /// <exception cref="ArgumentException">the number of input points is less
        /// than four or greater then the length of <code>points</code>, or the
        /// points appear to be coincident, colinear, or coplanar.</exception>
        public void Build(Point3d[] points, int nump)
        {
            if (nump < 4)

            {
                throw new ArgumentException("Less than four input points specified");
            }
            if (points.Length < nump)

            {
                throw new ArgumentException("Point array too small for specified number of points");
            }
            InitBuffers(nump);
            SetPoints(points, nump);
            BuildHull();
        }

        /// <summary>
        /// Triangulates any non-triangular hull faces. In some cases, due to
        /// precision issues, the resulting triangles may be very thin or small,
        /// and hence appear to be non-convex (this same limitation is present
        /// in <a href=http://www.qhull.org>qhull</a>).
        /// </summary>
        public void Triangulate()
        {
            double minArea = 1000 * charLength * DOUBLE_PREC;
            newFaces.Clear();
            foreach (var face in faces)
            {
                if (face.Mark == Face.VISIBLE)
                {
                    face.Triangulate(newFaces, minArea);
                    // splitFace (face);
                }
            }
            for (Face face = newFaces.First; face != null; face = face.Next)
            {
                faces.Add(face);
            }
        }

        // 	private void splitFace (Face face)
        // 	 {
        //  	   Face newFace = face.split();
        //  	   if (newFace != null)
        //  	    { newFaces.add (newFace);
        //  	      splitFace (newFace);
        //  	      splitFace (face);
        //  	    }
        // 	 }

        protected void InitBuffers(int nump)
        {
            if (pointBuffer.Length < nump)
            {
                Vertex[] newBuffer = new Vertex[nump];
                vertexPointIndices = new int[nump];
                for (int i = 0; i < pointBuffer.Length; i++)
                {
                    newBuffer[i] = pointBuffer[i];
                }
                for (int i = pointBuffer.Length; i < nump; i++)
                {
                    newBuffer[i] = new Vertex();
                }
                pointBuffer = newBuffer;
            }
            faces.Clear();
            claimed.Clear();
            numFaces = 0;
            numPoints = nump;
        }

        protected void SetPoints(double[] coords, int nump)
        {
            for (int i = 0; i < nump; i++)
            {
                Vertex vtx = pointBuffer[i];
                vtx.Point.Set(coords[i * 3 + 0], coords[i * 3 + 1], coords[i * 3 + 2]);
                vtx.Index = i;
            }
        }

        protected void SetPoints(Point3d[] pnts, int nump)
        {
            for (int i = 0; i < nump; i++)
            {
                Vertex vtx = pointBuffer[i];
                vtx.Point.Set(pnts[i]);
                vtx.Index = i;
            }
        }

        protected void ComputeMaxAndMin()
        {
            Vector3d max = new Vector3d();
            Vector3d min = new Vector3d();

            for (int i = 0; i < 3; i++)
            {
                maxVtxs[i] = minVtxs[i] = pointBuffer[0];
            }
            max.Set(pointBuffer[0].Point);
            min.Set(pointBuffer[0].Point);

            for (int i = 1; i < numPoints; i++)
            {
                Point3d pnt = pointBuffer[i].Point;
                if (pnt.x > max.x)
                {
                    max.x = pnt.x;
                    maxVtxs[0] = pointBuffer[i];
                }
                else if (pnt.x < min.x)
                {
                    min.x = pnt.x;
                    minVtxs[0] = pointBuffer[i];
                }
                if (pnt.y > max.y)
                {
                    max.y = pnt.y;
                    maxVtxs[1] = pointBuffer[i];
                }
                else if (pnt.y < min.y)
                {
                    min.y = pnt.y;
                    minVtxs[1] = pointBuffer[i];
                }
                if (pnt.z > max.z)
                {
                    max.z = pnt.z;
                    maxVtxs[2] = pointBuffer[i];
                }
                else if (pnt.z < min.z)
                {
                    min.z = pnt.z;
                    minVtxs[2] = pointBuffer[i];
                }
            }

            // this epsilon formula comes from QuickHull, and I'm
            // not about to quibble.
            charLength = Math.Max(max.x - min.x, max.y - min.y);
            charLength = Math.Max(max.z - min.z, charLength);
            if (ExplicitDistanceTolerance == AUTOMATIC_TOLERANCE)
            {
                DistanceTolerance =
               3 * DOUBLE_PREC * (Math.Max(Math.Abs(max.x), Math.Abs(min.x)) +
                      Math.Max(Math.Abs(max.y), Math.Abs(min.y)) +
                      Math.Max(Math.Abs(max.z), Math.Abs(min.z)));
            }
            else
            {
                DistanceTolerance = ExplicitDistanceTolerance;
            }
        }

        /// <summary>
        /// Creates the initial simplex from which the hull will be built.
        /// </summary>
        protected void CreateInitialSimplex()
        {
            double max = 0;
            int imax = 0;

            for (int i = 0; i < 3; i++)
            {
                double diff = maxVtxs[i].Point[i] - minVtxs[i].Point[i];
                if (diff > max)
                {
                    max = diff;
                    imax = i;
                }
            }

            if (max <= DistanceTolerance)
            {
                throw new ArgumentException("Input points appear to be coincident");
            }
            Vertex[]
            vtx = new Vertex[4];
            // set first two vertices to be those with the greatest
            // one dimensional separation

            vtx[0] = maxVtxs[imax];
            vtx[1] = minVtxs[imax];

            // set third vertex to be the vertex farthest from
            // the line between vtx0 and vtx1
            Vector3d u01 = new Vector3d();
            Vector3d diff02 = new Vector3d();
            Vector3d nrml = new Vector3d();
            Vector3d xprod = new Vector3d();
            double maxSqr = 0;
            u01.Sub(vtx[1].Point, vtx[0].Point);
            u01.Normalize();
            for (int i = 0; i < numPoints; i++)
            {
                diff02.Sub(pointBuffer[i].Point, vtx[0].Point);
                xprod.Cross(u01, diff02);
                double lenSqr = xprod.NormSquared;
                if (lenSqr > maxSqr &&
                pointBuffer[i] != vtx[0] &&  // paranoid
                pointBuffer[i] != vtx[1])
                {
                    maxSqr = lenSqr;
                    vtx[2] = pointBuffer[i];
                    nrml.Set(xprod);
                }
            }
            if (Math.Sqrt(maxSqr) <= 100 * DistanceTolerance)
            {
                throw new ArgumentException("Input points appear to be colinear");
            }
            nrml.Normalize();


            double maxDist = 0;
            double d0 = vtx[2].Point.Dot(nrml);
            for (int i = 0; i < numPoints; i++)
            {
                double dist = Math.Abs(pointBuffer[i].Point.Dot(nrml) - d0);
                if (dist > maxDist &&
                pointBuffer[i] != vtx[0] &&  // paranoid
                pointBuffer[i] != vtx[1] &&
                pointBuffer[i] != vtx[2])
                {
                    maxDist = dist;
                    vtx[3] = pointBuffer[i];
                }
            }
            if (Math.Abs(maxDist) <= 100 * DistanceTolerance)
            {
                throw new ArgumentException("Input points appear to be coplanar");
            }

            if (Debug)
            {
                Console.Out.WriteLine("initial vertices:");
                Console.Out.WriteLine(vtx[0].Index + ": " + vtx[0].Point);
                Console.Out.WriteLine(vtx[1].Index + ": " + vtx[1].Point);
                Console.Out.WriteLine(vtx[2].Index + ": " + vtx[2].Point);
                Console.Out.WriteLine(vtx[3].Index + ": " + vtx[3].Point);
            }

            Face[] tris = new Face[4];

            if (vtx[3].Point.Dot(nrml) - d0 < 0)
            {
                tris[0] = Face.CreateTriangle(vtx[0], vtx[1], vtx[2]);
                tris[1] = Face.CreateTriangle(vtx[3], vtx[1], vtx[0]);
                tris[2] = Face.CreateTriangle(vtx[3], vtx[2], vtx[1]);
                tris[3] = Face.CreateTriangle(vtx[3], vtx[0], vtx[2]);

                for (int i = 0; i < 3; i++)
                {
                    int k = (i + 1) % 3;
                    tris[i + 1].GetEdge(1).SetMutualOpposites(tris[k + 1].GetEdge(0));
                    tris[i + 1].GetEdge(2).SetMutualOpposites(tris[0].GetEdge(k));
                }
            }
            else
            {
                tris[0] = Face.CreateTriangle(vtx[0], vtx[2], vtx[1]);
                tris[1] = Face.CreateTriangle(vtx[3], vtx[0], vtx[1]);
                tris[2] = Face.CreateTriangle(vtx[3], vtx[1], vtx[2]);
                tris[3] = Face.CreateTriangle(vtx[3], vtx[2], vtx[0]);

                for (int i = 0; i < 3; i++)
                {
                    int k = (i + 1) % 3;
                    tris[i + 1].GetEdge(0).SetMutualOpposites(tris[k + 1].GetEdge(1));
                    tris[i + 1].GetEdge(2).SetMutualOpposites(tris[0].GetEdge((3 - i) % 3));
                }
            }


            for (int i = 0; i < 4; i++)
            {
                faces.Add(tris[i]);
            }

            for (int i = 0; i < numPoints; i++)
            {
                Vertex v = pointBuffer[i];

                if (v == vtx[0] || v == vtx[1] || v == vtx[2] || v == vtx[3])
                {
                    continue;
                }

                maxDist = DistanceTolerance;
                Face maxFace = null;
                for (int k = 0; k < 4; k++)
                {
                    double dist = tris[k].DistanceToPlane(v.Point);
                    if (dist > maxDist)
                    {
                        maxFace = tris[k];
                        maxDist = dist;
                    }
                }
                if (maxFace != null)
                {
                    AddPointToFace(v, maxFace);
                }
            }
        }

        /// <summary>
        /// The number of vertices in this hull.
        /// </summary>
        public int VertexCount =>  numVertices;


        /// <summary>
        /// Returns the vertex points in this hull.
        /// </summary>
        /// <returns>array of vertex points</returns>
        /// <see cref="GetVertices(double[])"/>
        /// <see cref="GetFaces()"/>
        public Point3d[] GetVertices()
        {
            Point3d[] vtxs = new Point3d[numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                vtxs[i] = pointBuffer[vertexPointIndices[i]].Point;
            }
            return vtxs;
        }

        /// <summary>
        /// Returns the coordinates of the vertex points of this hull.
        /// </summary>
        /// <param name="coords">returns the x, y, z coordinates of each vertex.
        /// This length of this array must be at least three times
        /// the number of vertices.</param>
        /// <returns>the number of vertices</returns>
        /// <see cref="GetVertices()"/>
        /// <see cref="GetFaces()"/>
        public int GetVertices(double[] coords)
        {
            for (int i = 0; i < numVertices; i++)
            {
                Point3d pnt = pointBuffer[vertexPointIndices[i]].Point;
                coords[i * 3 + 0] = pnt.x;
                coords[i * 3 + 1] = pnt.y;
                coords[i * 3 + 2] = pnt.z;
            }
            return numVertices;
        }

        /// <summary>
        /// Returns an array specifing the index of each hull vertex
        /// with respect to the original input points.
        /// </summary>
        /// <returns>vertex indices with respect to the original points</returns>
        public int[] GetVertexPointIndices()
        {
            int[] indices = new int[numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                indices[i] = vertexPointIndices[i];
            }
            return indices;
        }

        /// <summary>
        /// The number of faces in this hull.
        /// </summary>
        public int FaceCount => faces.Count;


        /// <summary>
        /// Returns the faces associated with this hull.
        /// 
        /// <p>Each face is represented by an integer array which gives the
        /// indices of the vertices. These indices are numbered
        /// relative to the
        /// hull vertices, are zero-based,
        /// and are arranged counter-clockwise. More control
        /// over the index format can be obtained using
        /// <see cref="GetFaces(int)"/> <see cref="GetVertices()"/>.
        /// </summary>
        /// 
        /// <returns>array of integer arrays, giving the vertex
        /// indices for each face.</returns>
        public int[][] GetFaces()
        {
            return GetFaces(0);
        }

        /// <summary>
        /// Returns the faces associated with this hull.
        /// 
        /// <p>Each face is represented by an integer array which gives the
        /// indices of the vertices. By default, these indices are numbered with
        /// respect to the hull vertices (as opposed to the input points), are
        /// zero-based, and are arranged counter-clockwise. However, this
        /// can be changed by setting <see cref="POINT_RELATIVE"/>,
        /// <see cref="INDEXED_FROM_ONE"/> or <see cref="CLOCKWISE"/>
        /// in the indexFlags parameter.
        /// </summary>
        /// 
        /// <param name="indexFlags">specifies index characteristics (0 results
        /// in the default)</param>
        /// <returns>array of integer arrays, giving the vertex
        /// indices for each face.</returns>
        /// <see cref="GetVertices()"/>
        public int[][] GetFaces(int indexFlags)
        {
            int[][] allFaces = new int[faces.Count][];
            int k = 0;
            foreach (var face in faces)
            {
                allFaces[k] = new int[face.VertexCount];
                GetFaceIndices(allFaces[k], face, indexFlags);
                k++;
            }
            return allFaces;
        }

        /// <summary>
        /// Prints the vertices and faces of this hull to the stream ps.
        /// 
        /// <p>
        /// This is done using the Alias Wavefront .obj file
        /// format, with the vertices printed first (each preceding by
        /// the letter <code>v</code>), followed by the vertex indices
        /// for each face (each
        /// preceded by the letter <code>f</code>).
        /// 
        /// <p>The face indices are numbered with respect to the hull vertices
        /// (as opposed to the input points), with a lowest index of 1, and are
        /// arranged counter-clockwise. More control over the index format can
        /// be obtained using <see cref="Print(TextWriter, int)"/>.
        /// {@link #print(PrintStream,int) print(ps,indexFlags)}.
        /// </summary>
        /// 
        /// <param name="ps">stream used for printing</param>
        /// <seealso cref="Print(TextWriter, int)"/>
        /// <seealso cref="GetVertices()"/>
        /// <seealso cref="GetFaces()"/>
        public void Print(TextWriter ps)
        {
            Print(ps, 0);
        }

        /// <summary>
        /// Prints the vertices and faces of this hull to the stream ps.
        /// 
        /// <p> This is done using the Alias Wavefront .obj file format, with
        /// the vertices printed first (each preceding by the letter
        /// <code>v</code>), followed by the vertex indices for each face (each
        /// preceded by the letter <code>f</code>).
        /// 
        /// <p>By default, the face indices are numbered with respect to the
        /// hull vertices (as opposed to the input points), with a lowest index
        /// of 1, and are arranged counter-clockwise. However, this
        /// can be changed by setting <see cref="POINT_RELATIVE"/>,
        /// <see cref="INDEXED_FROM_ONE"/>, <see cref="INDEXED_FROM_ZERO"/>,
        /// or <see cref="CLOCKWISE"/> in the indexFlags parameter.
        /// </summary>
        /// 
        /// <param name="ps">stream used for printing</param>
        /// <param name="indexFlags">specifies index characteristics
        /// (0 results in the default).</param>
        /// <seealso cref="GetVertices()"/>
        /// <seealso cref="GetFaces()"/>
        public void Print(TextWriter ps, int indexFlags)
        {
            if ((indexFlags & INDEXED_FROM_ZERO) == 0)
            {
                indexFlags |= INDEXED_FROM_ONE;
            }
            for (int i = 0; i < numVertices; i++)
            {
                Point3d pnt = pointBuffer[vertexPointIndices[i]].Point;
                ps.WriteLine("v " + pnt.x + " " + pnt.y + " " + pnt.z);
            }
            foreach (var face in faces)
            {
                int[] indices = new int[face.VertexCount];
                GetFaceIndices(indices, face, indexFlags);

                ps.Write("f");
                for (int k = 0; k < indices.Length; k++)
                {
                    ps.Write(" " + indices[k]);
                }
                ps.WriteLine("");
            }
        }

        private void GetFaceIndices(int[] indices, Face face, int flags)
        {
            bool ccw = ((flags & CLOCKWISE) == 0);
            bool indexedFromOne = ((flags & INDEXED_FROM_ONE) != 0);
            bool pointRelative = ((flags & POINT_RELATIVE) != 0);

            HalfEdge hedge = face.He0;
            int k = 0;
            do
            {
                int idx = hedge.Head.Index;
                if (pointRelative)
                {
                    idx = vertexPointIndices[idx];
                }
                if (indexedFromOne)
                {
                    idx++;
                }
                indices[k++] = idx;
                hedge = (ccw ? hedge.Next : hedge.Prev);
            }
            while (hedge != face.He0);
        }

        protected void ResolveUnclaimedPoints(FaceList newFaces)
        {
            Vertex vtxNext = unclaimed.First;
            for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
            {
                vtxNext = vtx.Next;

                double maxDist = DistanceTolerance;
                Face maxFace = null;
                for (Face newFace = newFaces.First; newFace != null;
                 newFace = newFace.Next)
                {
                    if (newFace.Mark == Face.VISIBLE)
                    {
                        double dist = newFace.DistanceToPlane(vtx.Point);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            maxFace = newFace;
                        }
                        if (maxDist > 1000 * DistanceTolerance)
                        {
                            break;
                        }
                    }
                }
                if (maxFace != null)
                {
                    AddPointToFace(vtx, maxFace);
                    if (Debug && vtx.Index == findIndex)
                    {
                        Console.Out.WriteLine($"{findIndex} CLAIMED BY {maxFace.VertexString}");
                    }
                }
                else
                {
                    if (Debug && vtx.Index == findIndex)
                    {
                        Console.Out.WriteLine($"{findIndex} DISCARDED");
                    }
                }
            }
        }

        protected void DeleteFacePoints(Face face, Face absorbingFace)
        {
            Vertex faceVtxs = RemoveAllPointsFromFace(face);
            if (faceVtxs != null)
            {
                if (absorbingFace == null)
                {
                    unclaimed.AddRange(faceVtxs);
                }
                else
                {
                    Vertex vtxNext = faceVtxs;
                    for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
                    {
                        vtxNext = vtx.Next;
                        double dist = absorbingFace.DistanceToPlane(vtx.Point);
                        if (dist > DistanceTolerance)
                        {
                            AddPointToFace(vtx, absorbingFace);
                        }
                        else
                        {
                            unclaimed.Add(vtx);
                        }
                    }
                }
            }
        }

        private const int NONCONVEX_WRT_LARGER_FACE = 1;
        private const int NONCONVEX = 2;

        protected double OppFaceDistance(HalfEdge he)
        {
            return he.Face.DistanceToPlane(he.Opposite.Face.Centroid);
        }

        private bool MergeAdjacent(Face face, int mergeType)
        {
            HalfEdge hedge = face.He0;

            bool convex = true;
            do
            {
                Face oppFace = hedge.OppositeFace;
                bool merge = false;
                double dist1;

                if (mergeType == NONCONVEX)
                { // then merge faces if they are definitively non-convex
                    if (OppFaceDistance(hedge) > -DistanceTolerance ||
                        OppFaceDistance(hedge.Opposite) > -DistanceTolerance)
                    {
                        merge = true;
                    }
                }
                else // mergeType == NONCONVEX_WRT_LARGER_FACE
                { // merge faces if they are parallel or non-convex
                  // wrt to the larger face; otherwise, just mark
                  // the face non-convex for the second pass.
                    if (face.Area > oppFace.Area)
                    {
                        if ((dist1 = OppFaceDistance(hedge)) > -DistanceTolerance)
                        {
                            merge = true;
                        }
                        else if (OppFaceDistance(hedge.Opposite) > -DistanceTolerance)
                        {
                            convex = false;
                        }
                    }
                    else
                    {
                        if (OppFaceDistance(hedge.Opposite) > -DistanceTolerance)
                        {
                            merge = true;
                        }
                        else if (OppFaceDistance(hedge) > -DistanceTolerance)
                        {
                            convex = false;
                        }
                    }
                }

                if (merge)
                {
                    if (Debug)
                    {
                        Console.Out.WriteLine($"  merging {face.VertexString}  and  {oppFace.VertexString}");
                    }

                    int numd = face.MergeAdjacentFace(hedge, discardedFaces);
                    for (int i = 0; i < numd; i++)
                    {
                        DeleteFacePoints(discardedFaces[i], face);
                    }
                    if (Debug)
                    {
                        Console.Out.WriteLine($"  result: {face.VertexString}");
                    }
                    return true;
                }
                hedge = hedge.Next;
            }
            while (hedge != face.He0);
            if (!convex)
            {
                face.Mark = Face.NON_CONVEX;
            }
            return false;
        }

        protected void CalculateHorizon(Point3d eyePnt, HalfEdge edge0, Face face, List<HalfEdge> horizon)
        {
            //	   oldFaces.add (face);
            DeleteFacePoints(face, null);
            face.Mark = Face.DELETED;
            if (Debug)
            {
                Console.Out.WriteLine($"  visiting face {face.VertexString}");
            }
            HalfEdge edge;
            if (edge0 == null)
            {
                edge0 = face.GetEdge(0);
                edge = edge0;
            }
            else
            {
                edge = edge0.Next;
            }
            do
            {
                Face oppFace = edge.OppositeFace;
                if (oppFace.Mark == Face.VISIBLE)
                {
                    if (oppFace.DistanceToPlane(eyePnt) > DistanceTolerance)
                    {
                        CalculateHorizon(eyePnt, edge.Opposite, oppFace, horizon);
                    }
                    else
                    {
                        horizon.Add(edge);
                        if (Debug)
                        {
                            Console.Out.WriteLine($"  adding horizon edge {edge.VertexString}");
                        }
                    }
                }
                edge = edge.Next;
            }
            while (edge != edge0);
        }

        private HalfEdge AddAdjoiningFace(
           Vertex eyeVtx, HalfEdge he)
        {
            Face face = Face.CreateTriangle(
               eyeVtx, he.Tail, he.Head);
            faces.Add(face);
            face.GetEdge(-1).SetMutualOpposites(he.Opposite);
            return face.GetEdge(0);
        }

        protected void AddNewFaces(
           FaceList newFaces, Vertex eyeVtx, List<HalfEdge> horizon)
        {
            newFaces.Clear();

            HalfEdge hedgeSidePrev = null;
            HalfEdge hedgeSideBegin = null;

            foreach (var horizonHe in horizon)
            {
                HalfEdge hedgeSide = AddAdjoiningFace(eyeVtx, horizonHe);
                if (Debug)
                {
                    Console.Out.WriteLine($"new face: {hedgeSide.Face.VertexString}");
                }
                if (hedgeSidePrev != null)
                {
                    hedgeSide.Next.SetMutualOpposites(hedgeSidePrev);
                }
                else
                {
                    hedgeSideBegin = hedgeSide;
                }
                newFaces.Add(hedgeSide.Face);
                hedgeSidePrev = hedgeSide;
            }
            hedgeSideBegin.Next.SetMutualOpposites(hedgeSidePrev);
        }

        protected Vertex NextPointToAdd()
        {
            if (!claimed.IsEmpty)
            {
                Face eyeFace = claimed.First.Face;
                Vertex eyeVtx = null;
                double maxDist = 0;
                for(    Vertex vtx = eyeFace.Outside;
                        vtx != null && vtx.Face == eyeFace;
                        vtx = vtx.Next)
                {
                    double dist = eyeFace.DistanceToPlane(vtx.Point);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        eyeVtx = vtx;
                    }
                }
                return eyeVtx;
            }
            else
            {
                return null;
            }
        }

        protected void AddPointToHull(Vertex eyeVtx)
        {
            horizon.Clear();
            unclaimed.Clear();

            if (Debug)
            {
                Console.Out.WriteLine($"Adding point: {eyeVtx.Index}");
                Console.Out.WriteLine($" which is {eyeVtx.Face.DistanceToPlane(eyeVtx.Point)} above face {eyeVtx.Face.VertexString}");
            }
            RemovePointFromFace(eyeVtx, eyeVtx.Face);
            CalculateHorizon(eyeVtx.Point, null, eyeVtx.Face, horizon);
            newFaces.Clear();
            AddNewFaces(newFaces, eyeVtx, horizon);

            // first merge pass ... merge faces which are non-convex
            // as determined by the larger face

            for (Face face = newFaces.First; face != null; face = face.Next)
            {
                if (face.Mark == Face.VISIBLE)
                {
                    while (MergeAdjacent(face, NONCONVEX_WRT_LARGER_FACE))
                        ;
                }
            }
            // second merge pass ... merge faces which are non-convex
            // wrt either face	     
            for (Face face = newFaces.First; face != null; face = face.Next)
            {
                if (face.Mark == Face.NON_CONVEX)
                {
                    face.Mark = Face.VISIBLE;
                    while (MergeAdjacent(face, NONCONVEX))
                        ;
                }
            }
            ResolveUnclaimedPoints(newFaces);
        }

        protected void BuildHull()
        {
            int cnt = 0;
            Vertex eyeVtx;

            ComputeMaxAndMin();
            CreateInitialSimplex();
            while ((eyeVtx = NextPointToAdd()) != null)
            {
                AddPointToHull(eyeVtx);
                cnt++;
                if (Debug)
                {
                    Console.Out.WriteLine($"iteration {cnt} done");
                }
            }
            ReindexFacesAndVertices();
            if (Debug)
            {
                Console.Out.WriteLine("hull done");
            }
        }

        private void MarkFaceVertices(Face face, int mark)
        {
            HalfEdge he0 = face.FirstEdge;
            HalfEdge he = he0;
            do
            {
                he.Head.Index = mark;
                he = he.Next;
            }
            while (he != he0);
        }

        protected void ReindexFacesAndVertices()
        {
            for (int i = 0; i < numPoints; i++)
            {
                pointBuffer[i].Index = -1;
            }

            // remove inactive faces and mark active vertices
            numFaces = 0;
            faces.RemoveAll(face => face.Mark != Face.VISIBLE);
            foreach (var face in faces)
            {
                MarkFaceVertices(face, 0);
                numFaces++;
            }

            // reindex vertices
            numVertices = 0;
            for (int i = 0; i < numPoints; i++)
            {
                Vertex vtx = pointBuffer[i];
                if (vtx.Index == 0)
                {
                    vertexPointIndices[numVertices] = i;
                    vtx.Index = numVertices++;
                }
            }
        }

        protected bool CheckFaceConvexity(
           Face face, double tol, TextWriter ps)
        {
            double dist;
            HalfEdge he = face.He0;
            do
            {
                face.CheckConsistency();
                // make sure edge is convex
                dist = OppFaceDistance(he);
                if (dist > tol)
                {
                    if (ps != null)
                    {
                        ps.WriteLine($"Edge {he.VertexString} non-convex by {dist}");
                    }
                    return false;
                }
                dist = OppFaceDistance(he.Opposite);
                if (dist > tol)
                {
                    if (ps != null)
                    {
                        ps.WriteLine($"Opposite edge {he.Opposite.VertexString} non-convex by {dist}");
                    }
                    return false;
                }
                if (he.Next.OppositeFace == he.OppositeFace)
                {
                    if (ps != null)
                    {
                        ps.WriteLine($"Redundant vertex {he.Head.Index} in face {face.VertexString}");
                    }
                    return false;
                }
                he = he.Next;
            }
            while (he != face.He0);
            return true;
        }

        protected bool CheckFaces(double tol, TextWriter ps)
        {
            // check edge convexity
            bool convex = true;
            foreach (var face in faces)
            {
                if (face.Mark == Face.VISIBLE)
                {
                    if (!CheckFaceConvexity(face, tol, ps))
                    {
                        convex = false;
                    }
                }
            }
            return convex;
        }

        /// <summary>
        /// Checks the correctness of the hull using the distance tolerance
        /// returned by <see cref="DistanceTolerance"/>; see
        /// <see cref="Check(TextWriter, double)"/> for details.
        /// </summary>
        /// 
        /// <param name="ps">print stream for diagnostic messages; may be
        /// set to <code>null</code> if no messages are desired.</param>
        /// <returns>true if the hull is valid</returns>
        /// <see cref="Check(TextWriter, double)"/>
        public bool Check(TextWriter ps)
        {
            return Check(ps, DistanceTolerance);
        }

        /// <summary>
        /// Checks the correctness of the hull. This is done by making sure that
        /// no faces are non-convex and that no points are outside any face.
        /// These tests are performed using the distance tolerance <i>tol</i>.
        /// Faces are considered non-convex if any edge is non-convex, and an
        /// edge is non-convex if the centroid of either adjoining face is more
        /// than <i>tol</i> above the plane of the other face. Similarly,
        /// a point is considered outside a face if its distance to that face's
        /// plane is more than 10 times <i>tol</i>.
        /// 
        /// <p>If the hull has been <see cref="Triangulate()">triangulated</see>,
        /// then this routine may fail if some of the resulting
        /// triangles are very small or thin.
        /// </summary>
        /// <param name="ps">print stream for diagnostic messages; may be
        /// set to <code>null</code> if no messages are desired.</param>
        /// <param name="tol">distance tolerance</param>
        /// <returns>true if the hull is valid</returns>
        /// <see cref="Check(TextWriter)"/>
        public bool Check(TextWriter ps, double tol)

        {
            // check to make sure all edges are fully connected
            // and that the edges are convex
            double dist;
            double pointTol = 10 * tol;

            if (!CheckFaces(DistanceTolerance, ps))
            {
                return false;
            }

            // check point inclusion

            for (int i = 0; i < numPoints; i++)
            {
                Point3d pnt = pointBuffer[i].Point;
                foreach (var face in faces)
                {
                    if (face.Mark == Face.VISIBLE)
                    {
                        dist = face.DistanceToPlane(pnt);
                        if (dist > pointTol)
                        {
                            if (ps != null)
                            {
                                ps.WriteLine($"Point {i} {dist} above face {face.VertexString}");
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
